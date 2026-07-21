using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.General;

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

        var result = await supportService.ReportAsync(request, userId, userEmail);
        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Signalement envoyé avec succès." });
    }
}
