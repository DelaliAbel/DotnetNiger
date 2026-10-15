using System.ComponentModel.DataAnnotations;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Signalement de contenu.</summary>
/// <param name="Reason">Motif du signalement.</param>
/// <param name="Details">Précisions facultatives.</param>
public sealed record CreateReportRequest(
    ReportReason Reason,
    [StringLength(1000)] string? Details = null);

/// <summary>État d'un signalement, réservé à la modération.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="TargetType">Type de cible.</param>
/// <param name="TargetId">Identifiant de la cible.</param>
/// <param name="Reason">Motif.</param>
/// <param name="Status">État de traitement.</param>
/// <param name="CreatedAtUtc">Date de création.</param>
public sealed record ReportResponse(
    Guid Id,
    ReportTargetType TargetType,
    Guid TargetId,
    ReportReason Reason,
    ReportStatus Status,
    DateTime CreatedAtUtc);

