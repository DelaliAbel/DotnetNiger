using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Boîte de réception des messages de contact.
/// </summary>
[Route("api/admin/contact")]
public sealed class AdminContactController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IEmailDispatcher emails,
    TimeProvider clock) : ApiControllerBase
{
    /// <summary>
    /// Liste les messages reçus, du plus récent au plus ancien.
    /// </summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="enAttente">Ne garder que les messages sans réponse.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de messages.</returns>
    /// <response code="200">Messages listés.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("messages")]
    [ProducesResponseType<PageResponse<ContactMessageResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PageResponse<ContactMessageResponse>>> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool enAttente = false,
        CancellationToken cancellationToken = default)
    {
        RequireAdministrateur(currentUser);

        var requete = unitOfWork.ReadRepository<ContactMessage>().Query;

        if (enAttente)
        {
            requete = requete.Where(message => message.HandledAtUtc == null);
        }

        return await PageOfAsync(
            requete.OrderByDescending(message => message.ReceivedAtUtc),
            new PageRequest(page, pageSize),
            ToResponse,
            cancellationToken);
    }

    /// <summary>
    /// Marque un message comme ayant reçu une réponse.
    /// </summary>
    /// <param name="id">Identifiant du message.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le message traité.</returns>
    /// <response code="200">Message traité.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Message introuvable.</response>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("messages/{id:guid}/handled")]
    [ProducesResponseType<ContactMessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactMessageResponse>> MarquerTraite(
        Guid id,
        CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var message = await unitOfWork.ReadRepository<ContactMessage>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (message is null)
        {
            return NotFound();
        }

        message.MarkHandled(clock.GetUtcNow().UtcDateTime);
        unitOfWork.Repository<ContactMessage>().Update(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(message));
    }

    /// <summary>
    /// Répond au demandeur par courriel, et marque le message comme traité.
    /// </summary>
    /// <param name="id">Identifiant du message.</param>
    /// <param name="request">Corps de la réponse.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le message, désormais traité.</returns>
    /// <remarks>
    /// La réponse part par le même relais que les autres courriels : elle passe donc par la
    /// file et par les modèles du site, et non par un envoi direct. Un lien
    /// <c>mailto:</c> aurait laissé la moitié du travail à l'administrateur — composer dans
    /// son logiciel de messagerie, sans trace dans la boîte, et en oubliant de marquer le
    /// message comme traité.
    /// <para>
    /// Le message n'est marqué traité qu'après acceptation par le relais. Dans le cas
    /// contraire, la boîte afficherait « répondu » alors que rien n'est parti — et
    /// l'administrateur ne le relirait jamais.
    /// </para>
    /// </remarks>
    /// <response code="200">Réponse expédiée, message traité.</response>
    /// <response code="400">Réponse vide, ou demandeur sans adresse.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Message introuvable.</response>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("messages/{id:guid}/reply")]
    [ProducesResponseType<ContactMessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactMessageResponse>> Repondre(
        Guid id,
        [FromBody] ReplyToContactRequest request,
        CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var message = await unitOfWork.ReadRepository<ContactMessage>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (message is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(message.Email))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Adresse manquante",
                Detail = "Ce message ne porte aucune adresse de retour : impossible d'y répondre.",
            });
        }

        var reponse = (request.Body ?? string.Empty).Trim();

        if (reponse.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Réponse vide",
                Detail = "Écrivez la réponse avant de l'envoyer.",
            });
        }

        // L'objet d'origine est conservé, préfixé d'un seul « Re: » : sans cela, une
        // réponse à une réponse produit « Re: Re: Re: » dans le fil du demandeur.
        var objet = message.Subject.StartsWith("Re:", StringComparison.OrdinalIgnoreCase)
            ? message.Subject
            : $"Re: {message.Subject}";

        await emails.DispatchAsync(
            EmailTemplateKeys.ContactReplied,
            message.Email,
            EmailValues.De(
                (EmailVariables.NomContact, message.Name),
                (EmailVariables.SujetContact, message.Subject),
                (EmailVariables.MessageReponse, reponse)),
            cancellationToken);

        message.MarkHandled(clock.GetUtcNow().UtcDateTime);
        unitOfWork.Repository<ContactMessage>().Update(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(message));
    }

    /// <summary>
    /// Retire un message de la boîte de réception.
    /// </summary>
    /// <param name="id">Identifiant du message.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <remarks>
    /// Le retrait est logique, comme partout ailleurs sur le site : le message disparaît des
    /// listes mais reste en base, se garder à l'existant d'un correspondant qu'il signalait
    /// ou qu'un signalement pourrait concerner.
    /// </remarks>
    /// <response code="204">Message retiré.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Message introuvable.</response>
    [HttpDelete]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("messages/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var message = await unitOfWork.ReadRepository<ContactMessage>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (message is null)
        {
            return NotFound();
        }

        unitOfWork.SoftDeletableRepository<ContactMessage>().Retire(
            message,
            clock.GetUtcNow().UtcDateTime,
            RequireAdministrateur(currentUser));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static ContactMessageResponse ToResponse(ContactMessage message) =>
        new(
            message.Id,
            message.Name,
            message.Email,
            message.Subject,
            message.Body,
            message.ReceivedAtUtc,
            message.HandledAtUtc);
}
