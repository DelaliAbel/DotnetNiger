using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Événements de la communauté et inscriptions.
/// </summary>
[Route("api/events")]
[Tags("Événements")]
public sealed class EventsController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    TimeProvider clock) : ApiControllerBase
{
    /// <summary>Liste les événements publiés, à venir par défaut.</summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="aVenir">Ne garder que les événements non terminés.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés d'événements.</returns>
    /// <response code="200">Page d'événements.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PageResponse<EventSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResponse<EventSummaryResponse>>> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool aVenir = true,
        CancellationToken cancellationToken = default)
    {
        var requete = unitOfWork.ReadRepository<CommunityEvent>().Query
            .Where(communityEvent => communityEvent.Status == PublicationStatus.Published);

        if (aVenir)
        {
            requete = requete.Where(communityEvent => communityEvent.EndsAtUtc >= DateTime.UtcNow);
        }

        requete = requete.OrderBy(communityEvent => communityEvent.StartsAtUtc);

        var demande = new PageRequest(page, pageSize);
        var total = await requete.CountAsync(cancellationToken);

        if (total == 0)
        {
            return ApiProjections.ToPage(
                PagedResultFactory.Empty<CommunityEvent>(demande),
                communityEvent => ApiProjections.ToSummary(communityEvent, 0));
        }

        var items = await requete
            .Skip(demande.Skip)
            .Take(demande.PageSize)
            .ToListAsync(cancellationToken);

        // Un comptage groupé par événement évite une requête par événement de la page.
        var confirmes = await unitOfWork.ReadRepository<EventRegistration>().Query
            .Where(inscription => inscription.Status == EventRegistrationStatus.Confirmed)
            .GroupBy(inscription => inscription.CommunityEventId)
            .Select(groupe => new { Id = groupe.Key, Total = groupe.Count() })
            .ToDictionaryAsync(resultat => resultat.Id, resultat => resultat.Total, cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<CommunityEvent>(items, demande.PageNumber, demande.PageSize, total),
            communityEvent => ApiProjections.ToSummary(
                communityEvent,
                confirmes.GetValueOrDefault(communityEvent.Id)));
    }

    /// <summary>Lit un événement publié.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement et son nombre d'inscriptions.</returns>
    /// <response code="200">Événement trouvé.</response>
    /// <response code="404">Événement introuvable ou non publié.</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<EventSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventSummaryResponse>> Lire(
        Guid id,
        CancellationToken cancellationToken)
    {
        var communityEvent = await unitOfWork.ReadRepository<CommunityEvent>().Query
            .FirstOrDefaultAsync(
                candidat => candidat.Id == id && candidat.Status == PublicationStatus.Published,
                cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        return Ok(ApiProjections.ToSummary(
            communityEvent,
            await ConfirmedCountAsync(id, cancellationToken)));
    }

    /// <summary>Inscrit le membre connecté à un événement.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le statut obtenu : confirmé ou en liste d'attente.</returns>
    /// <remarks>
    /// L'inscription et la lecture du compteur d'inscriptions se font dans une transaction :
    /// sans cela, deux requêtes simultanées pourraient dépasser la capacité.
    /// </remarks>
    /// <response code="201">Inscription enregistrée.</response>
    /// <response code="404">Événement introuvable.</response>
    /// <response code="409">Déjà inscrit, ou inscriptions closes.</response>
    [HttpPost("{id:guid}/registrations")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inscrire(Guid id, CancellationToken cancellationToken)
    {
        var membre = RequireUser(currentUser);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var communityEvent = await unitOfWork.ReadRepository<CommunityEvent>().Query
                .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken)
                ?? throw new DomainException("not_found", "Cet événement n'existe pas.");

            var maintenant = clock.GetUtcNow().UtcDateTime;
            var confirmes = await ConfirmedCountAsync(id, cancellationToken);

            // La règle d'accueil et la liste d'attente sont décidées par le domaine, qui
            // connaît les dates de clôture et la capacité. L'API ne fait que la constater.
            if (!communityEvent.CanRegister(confirmes, maintenant))
            {
                throw new DomainException(
                    "registration_closed",
                    "Les inscriptions à cet événement sont closes.");
            }

            var dejaInscrit = await unitOfWork.ReadRepository<EventRegistration>().Query
                .AnyAsync(
                    inscription => inscription.CommunityEventId == id && inscription.MemberId == membre,
                    cancellationToken);

            if (dejaInscrit)
            {
                throw new DomainException("already_exists", "Vous êtes déjà inscrit à cet événement.");
            }

            var statut = communityEvent.IsFull(confirmes)
                ? EventRegistrationStatus.Waitlisted
                : EventRegistrationStatus.Confirmed;

            unitOfWork.Repository<EventRegistration>().Add(
                EventRegistration.Create(id, membre, maintenant, statut));

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Created($"/api/events/{id}/registrations", new { eventId = id, status = statut });
        }
        catch
        {
            // Sans annulation, l'écriture en attente resterait visible pour la requête suivante.
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>Annule l'inscription du membre connecté.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse 204.</returns>
    /// <response code="204">Inscription annulée.</response>
    /// <response code="404">Aucune inscription à annuler.</response>
    [HttpDelete("{id:guid}/registrations")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnnulerSonInscription(Guid id, CancellationToken cancellationToken)
    {
        var membre = RequireUser(currentUser);

        var inscription = await unitOfWork.ReadRepository<EventRegistration>().Query
            .FirstOrDefaultAsync(
                candidat => candidat.CommunityEventId == id && candidat.MemberId == membre,
                cancellationToken);

        if (inscription is null)
        {
            return NotFound();
        }

        inscription.Cancel(clock.GetUtcNow().UtcDateTime);
        unitOfWork.Repository<EventRegistration>().Update(inscription);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private Task<int> ConfirmedCountAsync(Guid eventId, CancellationToken cancellationToken) =>
        unitOfWork.ReadRepository<EventRegistration>().Query
            .CountAsync(
                inscription => inscription.CommunityEventId == eventId
                    && inscription.Status == EventRegistrationStatus.Confirmed,
                cancellationToken);
}
