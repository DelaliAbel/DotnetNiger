using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.General;

/// <summary>Service de gestion des signalements utilisateur.</summary>
public interface ISupportService
{
    /// <summary>Envoie un signalement par email à l'équipe de support.</summary>
    Task<SupportReportResult> ReportAsync(SupportReportRequest request, string userId, string userEmail);
}
