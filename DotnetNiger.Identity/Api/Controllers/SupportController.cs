using Asp.Versioning;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/support")]
[Authorize]
public class SupportController(ISupportService supportService) : ControllerBase
{
    /// <summary>Signale un problème via le formulaire de support.</summary>
    [HttpPost("report")]
    public async Task<IActionResult> Report([FromBody] SupportReportRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "inconnu";
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "inconnu";
        var userTenant = User.FindFirst("tenant_id")?.Value ?? "inconnu";

        var result = await supportService.ReportAsync(request, userId, userEmail, userTenant);
        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Signalement envoyé avec succès." });
    }
}
