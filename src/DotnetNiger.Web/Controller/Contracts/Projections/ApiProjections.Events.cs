using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Infrastructure.Queries;

namespace DotnetNiger.Web.Controller.Contracts;

internal static partial class ApiProjections
{
    /// <summary>
    /// Projette un événement en résumé.
    /// </summary>
    /// <param name="communityEvent">Événement à projeter.</param>
    /// <param name="confirmes">Nombre d'inscriptions confirmées.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static EventSummaryResponse ToSummary(CommunityEvent communityEvent, int confirmes) =>
        new(
            communityEvent.Id,
            communityEvent.Slug,
            communityEvent.Title,
            communityEvent.Summary,
            communityEvent.Format,
            communityEvent.StartsAtUtc,
            communityEvent.EndsAtUtc,
            communityEvent.Location,
            communityEvent.OnlineUrl,
            communityEvent.Capacity,
            confirmes,
            communityEvent.CoverImageUrl);

    /// <summary>
    /// Projette un événement en résumé d'administration, état de publication compris.
    /// </summary>
    /// <param name="communityEvent">Événement à projeter.</param>
    /// <param name="confirmes">Nombre d'inscriptions confirmées.</param>
    /// <param name="createur">Nom du créateur, résolu hors requête.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static AdminEventSummaryResponse ToAdminSummary(
        CommunityEvent communityEvent,
        int confirmes,
        string createur) =>
        new(
            communityEvent.Id,
            communityEvent.Slug,
            communityEvent.Title,
            communityEvent.Summary,
            communityEvent.Format,
            communityEvent.StartsAtUtc,
            communityEvent.EndsAtUtc,
            communityEvent.Location,
            communityEvent.OnlineUrl,
            communityEvent.Capacity,
            confirmes,
            communityEvent.RegistrationClosesAtUtc,
            communityEvent.Status,
            createur,
            communityEvent.CoverImageUrl);

    /// <summary>
    /// Projette un événement en réponse complète.
    /// </summary>
    /// <param name="communityEvent">Événement à projeter.</param>
    /// <param name="confirmes">Nombre d'inscriptions confirmées.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static EventResponse ToDetail(CommunityEvent communityEvent, int confirmes) =>
        new(
            communityEvent.Id,
            communityEvent.Slug,
            communityEvent.Title,
            communityEvent.Summary,
            communityEvent.Description,
            communityEvent.Format,
            communityEvent.StartsAtUtc,
            communityEvent.EndsAtUtc,
            communityEvent.Location,
            communityEvent.OnlineUrl,
            communityEvent.Capacity,
            communityEvent.RegistrationClosesAtUtc,
            communityEvent.Status,
            confirmes,
            communityEvent.CoverImageUrl);

    /// <summary>
    /// Projette une inscription à un événement.
    /// </summary>
    /// <param name="registration">Inscription à projeter.</param>
    /// <param name="membreNom">Nom du membre inscrit, résolu hors requête.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static EventRegistrationResponse ToResponse(EventRegistration registration, string membreNom) =>
        new(
            registration.Id,
            registration.MemberId,
            membreNom,
            registration.Status,
            registration.RegisteredAtUtc,
            registration.CancelledAtUtc,
            registration.CheckedInAtUtc);

    /// <summary>
    /// Projette un événement lu en base en résumé.
    /// </summary>
    /// <param name="evenement">Événement à projeter.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static EventSummaryResponse ToSummary(EvenementLu evenement) =>
        new(
            evenement.Id,
            evenement.Slug,
            evenement.Titre,
            evenement.Resume,
            evenement.Format,
            evenement.DebutUtc,
            evenement.FinUtc,
            evenement.Lieu,
            evenement.LienParticipation,
            evenement.Capacite,
            evenement.Confirmes,
            evenement.ImageUrl);

}
