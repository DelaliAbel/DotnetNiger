using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Domain.Entities.Community;

/// <summary>
/// Inscription d'un membre à un événement.
/// </summary>
/// <remarks>
/// L'unicité de l'inscription est garantie par un index unique sur
/// (<see cref="CommunityEventId"/>, <see cref="MemberId"/>) : un membre ne peut pas
/// s'inscrire deux fois au même événement, même s'il annule puis se réinscrit.
/// </remarks>
public sealed class EventRegistration : TimestampedEntity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private EventRegistration()
    {
    }

    private EventRegistration(
        Guid communityEventId,
        Guid memberId,
        EventRegistrationStatus status,
        DateTime registeredAtUtc)
    {
        CommunityEventId = communityEventId;
        MemberId = memberId;
        Status = status;
        RegisteredAtUtc = registeredAtUtc;
    }

    /// <summary>
    /// Inscrit un membre à un événement.
    /// </summary>
    /// <param name="communityEventId">Identifiant de l'événement.</param>
    /// <param name="memberId">Identifiant du membre.</param>
    /// <param name="registeredAtUtc">Date d'inscription (UTC).</param>
    /// <param name="status">État initial : confirmée ou en liste d'attente.</param>
    /// <returns>Nouvelle inscription.</returns>
    public static EventRegistration Create(
        Guid communityEventId,
        Guid memberId,
        DateTime registeredAtUtc,
        EventRegistrationStatus status = EventRegistrationStatus.Confirmed)
    {
        DomainGuard.AgainstEmpty(communityEventId, nameof(communityEventId));
        DomainGuard.AgainstEmpty(memberId, nameof(memberId));

        return new EventRegistration(communityEventId, memberId, status, registeredAtUtc);
    }

    /// <summary>
    /// Obtient l'identifiant de l'événement.
    /// </summary>
    public Guid CommunityEventId { get; private set; }

    /// <summary>
    /// Obtient l'identifiant du membre inscrit.
    /// </summary>
    public Guid MemberId { get; private set; }

    /// <summary>
    /// Obtient l'état de l'inscription.
    /// </summary>
    public EventRegistrationStatus Status { get; private set; }

    /// <summary>
    /// Obtient la date d'inscription (UTC).
    /// </summary>
    public DateTime RegisteredAtUtc { get; private set; }

    /// <summary>
    /// Obtient la date d'annulation (UTC), ou <see langword="null"/> si l'inscription est active.
    /// </summary>
    public DateTime? CancelledAtUtc { get; private set; }

    /// <summary>
    /// Obtient la date d'arrivée sur site (UTC), ou <see langword="null"/> si le membre ne s'est pas présenté.
    /// </summary>
    public DateTime? CheckedInAtUtc { get; private set; }

    /// <summary>
    /// Annule l'inscription.
    /// </summary>
    /// <param name="cancelledAtUtc">Date d'annulation (UTC).</param>
    public void Cancel(DateTime cancelledAtUtc)
    {
        Status = EventRegistrationStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
    }

    /// <summary>
    /// Réactive une inscription annulée.
    /// </summary>
    public void Reactivate()
    {
        Status = EventRegistrationStatus.Confirmed;
        CancelledAtUtc = null;
    }

    /// <summary>
    /// Fait passer une inscription de la liste d'attente à une place confirmée.
    /// </summary>
    public void ConfirmFromWaitlist()
    {
        Status = EventRegistrationStatus.Confirmed;
        CancelledAtUtc = null;
    }

    /// <summary>
    /// Enregistre l'arrivée du membre sur place.
    /// </summary>
    /// <param name="checkedInAtUtc">Date d'arrivée (UTC).</param>
    public void CheckIn(DateTime checkedInAtUtc) => CheckedInAtUtc = checkedInAtUtc;
}
