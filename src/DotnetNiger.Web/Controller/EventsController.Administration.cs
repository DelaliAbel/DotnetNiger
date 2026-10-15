using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Cycle de vie d'un événement côté administration : création, mise à jour, publication,
/// archivage, retour au brouillon et retrait.
/// </summary>
/// <remarks>
/// Un événement est une initiative de l'association : sa création est réservée à
/// l'administration, contrairement aux ressources et aux articles, que tout membre peut
/// proposer. Les événements déjà annoncés font l'objet d'une information, parce qu'ils
/// concernent des personnes ; les brouillons restent muets.
/// </remarks>
public sealed partial class EventsController
{
    /// <summary>Crée un événement, en brouillon ou publié.</summary>
    /// <param name="request">Contenu de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement créé.</returns>
    /// <remarks>
    /// Un événement est une initiative de l'association : sa création est réservée à
    /// l'administration, contrairement aux ressources et aux articles, que tout membre
    /// peut proposer.
    /// </remarks>
    /// <response code="201">Événement créé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<EventResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventResponse>> Creer(
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        var auteur = RequireAdministrateur(currentUser);

        var communityEvent = new CommunityEvent(
            auteur,
            request.Title,
            request.Summary,
            request.Description,
            request.Format,
            request.StartsAtUtc,
            request.EndsAtUtc);

        communityEvent.SetLocation(request.Location);
        communityEvent.SetOnlineUrl(request.OnlineUrl);
        communityEvent.SetCapacity(request.Capacity);
        communityEvent.SetRegistrationClosesAt(request.RegistrationClosesAtUtc);
        communityEvent.SetCoverImage(request.CoverImageUrl);

        // Le type n'est pas obligatoire : une rencontre sans type reste décrite par son
        // titre et sa date. Il n'est donc posé que s'il est renseigné — le domaine
        // refuserait une valeur vide.
        if (!string.IsNullOrWhiteSpace(request.Kind))
        {
            communityEvent.SetKind(request.Kind);
        }

        var publie = request.Publish;

        if (publie)
        {
            communityEvent.Publish(clock.GetUtcNow().UtcDateTime);
        }

        unitOfWork.Repository<CommunityEvent>().Add(communityEvent);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (publie)
        {
            await newsletter.DiffuserAsync(NouvellePublication.De(communityEvent), cancellationToken);
        }

        return CreatedAtAction(nameof(Lire), new { id = communityEvent.Id }, ApiProjections.ToDetail(communityEvent, 0));
    }

    /// <summary>Met à jour un événement, y compris son image de couverture.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="request">Nouveau contenu.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement mis à jour.</returns>
    /// <response code="200">Événement mis à jour.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Événement introuvable.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> MettreAJour(
        Guid id,
        [FromBody] UpdateEventRequest request,
        CancellationToken cancellationToken)
    {
        var communityEvent = await ChargerEvenement(id, cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        communityEvent.UpdateDetails(
            request.Title,
            request.Summary,
            request.Description,
            request.StartsAtUtc,
            request.EndsAtUtc);

        communityEvent.SetLocation(request.Location);
        communityEvent.SetOnlineUrl(request.OnlineUrl);
        communityEvent.SetCapacity(request.Capacity);
        communityEvent.SetRegistrationClosesAt(request.RegistrationClosesAtUtc);
        communityEvent.SetCoverImage(request.CoverImageUrl);

        if (!string.IsNullOrWhiteSpace(request.Kind))
        {
            communityEvent.SetKind(request.Kind);
        }

        // Seule la modification d'un événement déjà annoncé concerne des personnes : un
        // brouillon n'a personne à prévenir, et l'information sur sa nouvelle date n'est
        // encore connue de personne.
        var dejaPublie = communityEvent.Status == PublicationStatus.Published;

        unitOfWork.Repository<CommunityEvent>().Update(communityEvent);

        if (dejaPublie)
        {
            await PrevenirModificationAsync(communityEvent, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToDetail(communityEvent, await ConfirmedCountAsync(id, cancellationToken)));
    }

    /// <summary>Publie un événement en brouillon.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement publié.</returns>
    /// <response code="200">Événement publié.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Événement introuvable.</response>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> Publier(Guid id, CancellationToken cancellationToken)
    {
        var communityEvent = await ChargerEvenement(id, cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        var dejaPublie = communityEvent.PublishedAtUtc is not null;
        communityEvent.Publish(clock.GetUtcNow().UtcDateTime);

        unitOfWork.Repository<CommunityEvent>().Update(communityEvent);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!dejaPublie)
        {
            await newsletter.DiffuserAsync(NouvellePublication.De(communityEvent), cancellationToken);
        }

        return Ok(ApiProjections.ToDetail(communityEvent, await ConfirmedCountAsync(id, cancellationToken)));
    }

    /// <summary>Archive un événement, le retirant du calendrier sans le supprimer.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement archivé.</returns>
    /// <response code="200">Événement archivé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Événement introuvable.</response>
    [HttpPost("{id:guid}/archive")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> Archiver(Guid id, CancellationToken cancellationToken)
    {
        var communityEvent = await ChargerEvenement(id, cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        communityEvent.Archive();

        unitOfWork.Repository<CommunityEvent>().Update(communityEvent);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToDetail(communityEvent, await ConfirmedCountAsync(id, cancellationToken)));
    }

    /// <summary>Repasse un événement en brouillon, le retirant du calendrier sans le supprimer.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement repassé en brouillon.</returns>
    /// <response code="200">Événement repassé en brouillon.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Événement introuvable.</response>
    [HttpPost("{id:guid}/draft")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> RepasserEnBrouillon(
        Guid id,
        CancellationToken cancellationToken)
    {
        var communityEvent = await ChargerEvenement(id, cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        communityEvent.ReturnToDraft();

        unitOfWork.Repository<CommunityEvent>().Update(communityEvent);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToDetail(communityEvent, await ConfirmedCountAsync(id, cancellationToken)));
    }

    /// <summary>Retire un événement du site sans en perdre l'historique.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <remarks>
    /// Les inscriptions déjà prises sont conservées : elles font partie de l'historique de
    /// l'événement, et les participants doivent pouvoir retrouver l'événement sur lequel
    /// ils ont commenté.
    /// </remarks>
    /// <response code="204">Événement retiré.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Événement introuvable.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken cancellationToken)
    {
        var communityEvent = await ChargerEvenement(id, cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        var auteur = RequireAdministrateur(currentUser);
        var maintenant = clock.GetUtcNow().UtcDateTime;

        // Un événement retiré n'aura pas lieu : les personnes inscrites doivent l'apprendre
        // de l'administration et non enconstater le jour venu.
        await PrevenirAnnulationAsync(communityEvent, cancellationToken);

        unitOfWork.SoftDeletableRepository<CommunityEvent>().Retire(communityEvent, maintenant, auteur);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}