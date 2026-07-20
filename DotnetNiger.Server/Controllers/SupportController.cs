using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[Route("api/support")]
[Authorize]
public class SupportController(ISupportService supportService) : ControllerBase
{
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
