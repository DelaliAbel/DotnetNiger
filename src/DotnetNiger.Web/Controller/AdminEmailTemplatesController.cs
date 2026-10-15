using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using DotnetNiger.Infrastructure.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Essai des modèles de courriel, réservé à l'administration.
/// </summary>
[Route("api/admin/email-templates")]
[Tags("Administration des modèles de courriel")]
[Authorize(Roles = RoleNames.Admin)]
public sealed partial class AdminEmailTemplatesController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    UserManager<ApplicationUser> userManager,
    IOutbox outbox,
    IOptions<EmailOptions> emailOptions,
    DotnetNiger.Web.Services.Reglages.ServiceReglages reglages,
    ILogger<AdminEmailTemplatesController> logger) : ApiControllerBase
{
    /// <summary>
    /// Envoie un modèle de courriel à l'administrateur connecté, avec des données d'exemple.
    /// </summary>
    /// <param name="cle">Clé du modèle, telle que dans <c>EmailTemplateKeys</c>.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le message mis en file.</returns>
    /// <response code="202">Message mis en file d'attente.</response>
    /// <response code="400">Le compte administrateur n'a pas d'adresse électronique.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Aucun modèle ne correspond à la clé.</response>
    /// <response code="429">Trop de demandes.</response>
    /// <remarks>
    /// La destination n'est pas un paramètre : c'est toujours l'adresse du compte connecté.
    /// Un paramètre d'adresse ferait de ce point de terminaison un relais d'envoi ouvert —
    /// n'importe quel administrateur pourrait écrire à n'importe qui, autant de fois qu'il
    /// le souhaite. En ne l'exposant pas, l'abus n'a pas de forme à écrire.
    /// <para>
    /// Le message part avec les valeurs d'exemple du modèle : « Awa Traoré », une date
    /// future, une adresse de lien de démonstration. Aucun clic sur ces liens n'a d'effet,
    /// les jetons étant fictifs.
    /// </para>
    /// <para>
    /// L'envoi passe par la file d'attente. Si le serveur de courriel est injoignable, le
    /// message reste en base et repartira tout seul : un essai raté n'est jamais perdu.
    /// </para>
    /// </remarks>
    [HttpPost("{cle}/test")]
    [EnableRateLimiting("courriel")]
    [ProducesResponseType<EmailTestQueuedResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailTestQueuedResponse>> EnvoyerEssai(
        string cle,
        CancellationToken cancellationToken)
    {
        var administrateurId = RequireAdministrateur(currentUser);

        if (!EmailTemplateCatalog.Essayer(cle, out var modele) || modele is null)
        {
            throw new DomainException(
                "not_found",
                $"Aucun modèle de courriel ne correspond à « {cle} ».");
        }

        var administrateur = await userManager.FindByIdAsync(administrateurId.ToString())
            ?? throw new DomainException("not_found", "Le compte administrateur est introuvable.");

        if (string.IsNullOrWhiteSpace(administrateur.Email))
        {
            throw new DomainException(
                "email_address_missing",
                "Le compte administrateur n'a pas d'adresse électronique : l'essai est impossible.");
        }

        var destinataire = administrateur.Email;
        var message = modele.RendreExemple(
            EmailBranding.DepuisOptions(
                emailOptions.Value,
                (await reglages.LireAsync(cancellationToken)).Logo));

        await outbox.EnqueueAsync(
            modele.Cle,
            new QueuedEmail(destinataire, message),
            cancellationToken);

        // La validation est ici, et non dans la file : le message et l'essai sont inscrits
        // dans la même transaction. Si l'écriture échoue, rien n'est prétendu à l'appelant.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        EssaiMisEnFile(logger, modele.Cle, destinataire);

        return Accepted(new EmailTestQueuedResponse(
            modele.Cle,
            modele.Titre,
            destinataire,
            message.Subject,
            message.TextBody));
    }

    [LoggerMessage(
        EventId = 80,
        Level = LogLevel.Information,
        Message = "Essai du modèle {Modele} mis en file pour {Destinataire}")]
    private static partial void EssaiMisEnFile(ILogger logger, string modele, string destinataire);
}
