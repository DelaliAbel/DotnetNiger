using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
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
