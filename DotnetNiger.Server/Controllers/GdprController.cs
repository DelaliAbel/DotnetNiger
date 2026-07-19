using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static OpenIddict.Abstractions.OpenIddictConstants;
using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Infrastructure.Services;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/account")]
[Authorize]
public class GdprController : ControllerBase
{
    private readonly GdprService _gdprService;
    private readonly GdprExportService _gdprExportService;

    public GdprController(GdprService gdprService, GdprExportService gdprExportService)
    {
        _gdprService = gdprService;
        _gdprExportService = gdprExportService;
    }

    [HttpPost("consent")]
    public async Task<IActionResult> RecordConsent([FromBody] ConsentRequest request)
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();

        await _gdprService.RecordConsentAsync(Guid.Parse(userId),
            request.ConsentType, request.ConsentVersion, request.Granted,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString());
        return Ok(new { message = "Consentement enregistré." });
    }

    [HttpGet("consent")]
    public async Task<ActionResult<List<ConsentResponse>>> GetConsentHistory()
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();

        var consents = await _gdprService.GetLatestConsentsAsync(Guid.Parse(userId));
        return Ok(consents);
    }

    [HttpGet("data")]
    public async Task<IActionResult> ExportData()
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();

        var (zipData, userName) = await _gdprExportService.ExportUserDataAsync(Guid.Parse(userId));
        return File(zipData, "application/zip", $"{userName}-export-{DateTime.UtcNow:yyyy-MM-dd}.zip");
    }

    [HttpPost("forget-me")]
    public async Task<ActionResult<ForgetMeResponse>> ForgetMe()
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();

        await _gdprExportService.ForgetMeAsync(Guid.Parse(userId));
        return Ok(new ForgetMeResponse("Vos données ont été anonymisées conformément au RGPD.", DateTime.UtcNow));
    }
}
