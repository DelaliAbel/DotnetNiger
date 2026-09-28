using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Gestion des abonnements à la lettre d'information et diffusion.
/// </summary>
[Route("api/admin/newsletter")]
public sealed partial class AdminNewsletterController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IEmailDispatcher emails,
    IOptions<EmailOptions> emailOptions,
    ILogger<AdminNewsletterController> logger) : ApiControllerBase
{
    /// <summary>
    /// Liste les abonnements.
    /// </summary>
    /// <param name="statut">État à filtrer, ou <see langword="null"/> pour tous.</param>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page d'abonnements.</returns>
    /// <response code="200">Abonnements listés.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("subscribers")]
    [ProducesResponseType<PageResponse<NewsletterSubscriptionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PageResponse<NewsletterSubscriptionResponse>>> Lister(
        [FromQuery] NewsletterStatus? statut = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        RequireAdministrateur(currentUser);

        var requete = unitOfWork.ReadRepository<NewsletterSubscription>().Query;

        if (statut is not null)
        {
            requete = requete.Where(abonnement => abonnement.Status == statut.Value);
        }

        return await PageOfAsync(
            requete.OrderByDescending(abonnement => abonnement.RequestedAtUtc),
            new PageRequest(page, pageSize),
            ToResponse,
            cancellationToken);
    }

    /// <summary>
    /// Diffuse un nouveau contenu aux abonnements confirmés.
    /// </summary>
    /// <param name="request">Contenu à diffuser.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le nombre de messages mis en file.</returns>
    /// <remarks>
    /// Un message par destinataire, jamais une liste en copie cachée : c'est ce qui permet
    /// à qui se désabonne de ne plus recevoir que lui-même, et c'est la condition pour que
    /// le désabonnement un par un reste possible.
    /// </remarks>
    /// <response code="202">Diffusion mise en file.</response>
    /// <response code="400">Contenu incomplet.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("send")]
    [ProducesResponseType<NewsletterSendResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<NewsletterSendResponse>> Diffuser(
        [FromBody] SendNewsletterRequest request,
        CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        // Seuls les abonnements confirmés reçoivent la lettre : une demande en attente de
        // confirmation est une adresse qui n'a pas encore dit oui.
        var abonnes = await unitOfWork.ReadRepository<NewsletterSubscription>().Query
            .Where(abonnement => abonnement.Status == NewsletterStatus.Confirmed)
            .OrderBy(abonnement => abonnement.ConfirmedAtUtc)
            .Select(abonnement => new { abonnement.Email, abonnement.UnsubscribeKey })
            .ToListAsync(cancellationToken);

        foreach (var abonne in abonnes)
        {
            await emails.DispatchAsync(
                EmailTemplateKeys.NewsletterNewContent,
                abonne.Email,
                EmailValues.De(
                    (EmailVariables.TypeContenu, request.ContentType),
                    (EmailVariables.TitreContenu, request.Title),
                    (EmailVariables.ResumeContenu, request.Summary),
                    (EmailVariables.UrlContenu, request.Url),
                    (EmailVariables.AuteurContenu, request.Author ?? string.Empty),
                    (EmailVariables.UrlDesabonnement, DesabonnementUrl(abonne.UnsubscribeKey))),
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        DiffusionMiseEnFile(logger, abonnes.Count);

        return Accepted(new NewsletterSendResponse(abonnes.Count));
    }

    private string DesabonnementUrl(string cleDesabonnement) =>
        $"{emailOptions.Value.BuildUrl("/api/newsletter/unsubscribe")}?key={Uri.EscapeDataString(cleDesabonnement)}";

    private static NewsletterSubscriptionResponse ToResponse(NewsletterSubscription abonnement) =>
        new(
            abonnement.Id,
            abonnement.Email,
            abonnement.Status,
            abonnement.RequestedAtUtc,
            abonnement.ConfirmedAtUtc,
            abonnement.UnsubscribedAtUtc);

    [LoggerMessage(
        EventId = 124,
        Level = LogLevel.Information,
        Message = "Diffusion de la lettre mise en file pour {Nombre} abonnes")]
    private static partial void DiffusionMiseEnFile(ILogger logger, int nombre);
}
