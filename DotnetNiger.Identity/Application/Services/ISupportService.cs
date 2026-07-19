using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services;

/// <summary>Service de gestion des signalements utilisateur.</summary>
public interface ISupportService
{
    /// <summary>Envoie un signalement par email à l'équipe de support.</summary>
    Task<SupportReportResult> ReportAsync(SupportReportRequest request, string userId, string userEmail, string userTenant);
}
