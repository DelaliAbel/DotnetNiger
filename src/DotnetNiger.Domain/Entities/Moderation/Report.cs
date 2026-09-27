using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Entities.Moderation;

/// <summary>
/// Signalement d'un contenu par un membre, traité par l'administration.
/// </summary>
/// <remarks>
/// Il n'existe pas de rôle de modérateur : le traitement est réservé aux administrateurs.
/// <para>
/// Un signalement référence sa cible par type et identifiant plutôt que par une clé
/// étrangère, afin de ne pas ajouter de colonne à chaque nouveau type de contenu
/// modérable. Un index unique sur la cible et le rapporteur empêche le spam de signalements
/// répétés depuis le même compte.
/// </para>
/// </remarks>
public sealed class Report : TimestampedEntity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private Report()
    {
    }

    /// <summary>
    /// Initialise un nouveau signalement à l'état ouvert.
    /// </summary>
    /// <param name="targetType">Nature du contenu signalé.</param>
    /// <param name="targetId">Identifiant du contenu signalé.</param>
    /// <param name="reason">Motif du signalement.</param>
    /// <param name="reporterId">Identifiant du membre auteur du signalement.</param>
    /// <param name="details">Précisions facultatives.</param>
    public Report(ReportTargetType targetType, Guid targetId, ReportReason reason, Guid reporterId, string? details = null)
    {
        TargetType = targetType;
        TargetId = DomainGuard.AgainstEmpty(targetId, nameof(targetId));
        Reason = reason;
        ReporterId = DomainGuard.AgainstEmpty(reporterId, nameof(reporterId));
        Details = DomainGuard.OptionalAgainstTooLong(details, FieldLimits.Body, nameof(details));
        Status = ReportStatus.Open;
    }

    /// <summary>
    /// Obtient la nature du contenu signalé.
    /// </summary>
    public ReportTargetType TargetType { get; private set; }

    /// <summary>
    /// Obtient l'identifiant du contenu signalé.
    /// </summary>
    public Guid TargetId { get; private set; }

    /// <summary>
    /// Obtient le motif du signalement.
    /// </summary>
    public ReportReason Reason { get; private set; }

    /// <summary>
    /// Obtient les précisions fournies par le membre.
    /// </summary>
    public string? Details { get; private set; }

    /// <summary>
    /// Obtient l'état de traitement.
    /// </summary>
    public ReportStatus Status { get; private set; }

    /// <summary>
    /// Obtient l'identifiant du membre ayant signalé.
    /// </summary>
    public Guid ReporterId { get; private set; }

    /// <summary>
    /// Obtient l'identifiant de l'administrateur ayant pris en charge le signalement.
    /// </summary>
    public Guid? HandledById { get; private set; }

    /// <summary>
    /// Obtient la date de clôture du signalement (UTC).
    /// </summary>
    public DateTime? HandledAtUtc { get; private set; }

    /// <summary>
    /// Obtient la conclusion de l'administration.
    /// </summary>
    public string? ResolutionNote { get; private set; }

    /// <summary>
    /// Prend en charge le signalement.
    /// </summary>
    /// <param name="handledById">Identifiant de l'administrateur.</param>
    public void StartReview(Guid handledById)
    {
        DomainGuard.AgainstEmpty(handledById, nameof(handledById));
        EnsureNotClosed();

        Status = ReportStatus.UnderReview;
        HandledById = handledById;
    }

    /// <summary>
    /// Clôt le signalement en le confirmant : une action de modération a été appliquée.
    /// </summary>
    /// <param name="handledById">Identifiant de l'administrateur.</param>
    /// <param name="note">Conclusion, facultative.</param>
    /// <param name="handledAtUtc">Date de clôture (UTC).</param>
    public void Resolve(Guid handledById, string? note, DateTime handledAtUtc)
    {
        DomainGuard.AgainstEmpty(handledById, nameof(handledById));
        Close(ReportStatus.Resolved, handledById, note, handledAtUtc);
    }

    /// <summary>
    /// Clôt le signalement en le déclarant infondé.
    /// </summary>
    /// <param name="handledById">Identifiant de l'administrateur.</param>
    /// <param name="note">Conclusion, facultative.</param>
    /// <param name="handledAtUtc">Date de clôture (UTC).</param>
    public void Dismiss(Guid handledById, string? note, DateTime handledAtUtc)
    {
        DomainGuard.AgainstEmpty(handledById, nameof(handledById));
        Close(ReportStatus.Dismissed, handledById, note, handledAtUtc);
    }

    private void Close(ReportStatus status, Guid handledById, string? note, DateTime handledAtUtc)
    {
        EnsureNotClosed();

        Status = status;
        HandledById = handledById;
        HandledAtUtc = handledAtUtc;
        ResolutionNote = DomainGuard.OptionalAgainstTooLong(note, FieldLimits.Body, nameof(note));
    }

    private void EnsureNotClosed()
    {
        if (Status is ReportStatus.Resolved or ReportStatus.Dismissed)
        {
            throw new DomainException(
                "report_already_closed",
                "Ce signalement est déjà clôturé et ne peut plus changer d'état.");
        }
    }
}
