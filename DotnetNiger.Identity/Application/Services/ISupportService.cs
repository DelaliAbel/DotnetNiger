using DotnetNiger.Identity.Application.DTOs.Requests;

namespace DotnetNiger.Identity.Application.Services;

/// <summary>Service de gestion des signalements utilisateur.</summary>
public interface ISupportService
{
    /// <summary>Envoie un signalement par email à l'équipe de support.</summary>
    Task<SupportReportResult> ReportAsync(SupportReportRequest request, string userId, string userEmail, string userTenant);
}

/// <summary>Résultat de l'envoi d'un signalement.</summary>
public class SupportReportResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
}
