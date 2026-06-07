using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/support")]
[Authorize]
public class SupportController : ControllerBase
{
    private readonly EmailSender _emailSender;
    private readonly SmtpOptions _smtp;
    private readonly ILogger<SupportController> _logger;

    public SupportController(
        EmailSender emailSender,
        IOptions<SmtpOptions> smtp,
        ILogger<SupportController> logger)
    {
        _emailSender = emailSender;
        _smtp = smtp.Value;
        _logger = logger;
    }

    [HttpPost("report")]
    public async Task<IActionResult> Report([FromBody] SupportReportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            return BadRequest(new { error = "Le titre et la description sont requis." });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "inconnu";
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "inconnu";
        var userTenant = User.FindFirst("tenant_id")?.Value ?? "inconnu";

        var supportEmail = _smtp.SupportEmail;
        if (string.IsNullOrEmpty(supportEmail))
            supportEmail = "koffilevis21@gmail.com";

        var subject = $"[Signalement] {request.Title}";
        var body = $@"<h3>Nouveau signalement de bug</h3>
<table style='border-collapse:collapse;width:100%'>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Titre</td><td style='padding:8px;border:1px solid #ddd'>{request.Title}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Type</td><td style='padding:8px;border:1px solid #ddd'>{request.Type ?? "bug"}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Description</td><td style='padding:8px;border:1px solid #ddd'>{request.Description}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Étapes pour reproduire</td><td style='padding:8px;border:1px solid #ddd'>{request.Steps ?? "Non fourni"}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Page URL</td><td style='padding:8px;border:1px solid #ddd'>{request.PageUrl ?? "Non fourni"}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Navigateur/OS</td><td style='padding:8px;border:1px solid #ddd'>{request.UserAgent ?? "Non fourni"}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Utilisateur ID</td><td style='padding:8px;border:1px solid #ddd'>{userId}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Email</td><td style='padding:8px;border:1px solid #ddd'>{userEmail}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Tenant</td><td style='padding:8px;border:1px solid #ddd'>{userTenant}</td></tr>
</table>";

        try
        {
            await _emailSender.SendEmailAsync(supportEmail, subject, body);
            _logger.LogInformation("Support report sent: {Title} from user {UserId}", request.Title, userId);
            return Ok(new { message = "Signalement envoyé avec succès." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send support report");
            return StatusCode(500, new { error = "Erreur lors de l'envoi du signalement." });
        }
    }
}

public class SupportReportRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Type { get; set; }
    public string? Steps { get; set; }
    public string? PageUrl { get; set; }
    public string? UserAgent { get; set; }
}
