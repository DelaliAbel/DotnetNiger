using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.General;

/// <summary>Contrôleur de support technique pour les signalements.</summary>
[ApiController]
[Route("api/support")]
[Authorize]
public class SupportController(ISupportService supportService) : BaseController
{
    /// <summary>Envoie un signalement de support.</summary>
    [HttpPost("report")]
    public async Task<IActionResult> Report([FromBody] SupportReportRequest request, CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "inconnu";
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "inconnu";

        var result = await supportService.ReportAsync(request, userId, userEmail, ct);
        if (!result.Success)
            return BadRequest(result.Error ?? "Erreur inconnue");

        return Success<object?>(null, "Signalement envoyé avec succès.");
    }
}
