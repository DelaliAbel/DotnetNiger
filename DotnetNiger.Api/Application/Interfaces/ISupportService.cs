using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Service de gestion des signalements utilisateur.</summary>
public interface ISupportService
{
    /// <summary>Envoie un signalement par email à l'équipe de support.</summary>
    Task<SupportReportResult> ReportAsync(SupportReportRequest request, string userId, string userEmail, CancellationToken ct = default);
}
