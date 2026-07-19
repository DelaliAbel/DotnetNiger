using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DotnetNiger.Domain.Email;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

/// <summary>Service de gestion des signalements utilisateur.</summary>
public class SupportService : ISupportService
{
    private readonly EmailSender _emailSender;
    private readonly SmtpOptions _smtp;
    private readonly ILogger<SupportService> _logger;

    public SupportService(EmailSender emailSender, IOptions<SmtpOptions> smtp, ILogger<SupportService> logger)
    {
        _emailSender = emailSender;
        _smtp = smtp.Value;
        _logger = logger;
    }

    public async Task<SupportReportResult> ReportAsync(SupportReportRequest request, string userId, string userEmail, string userTenant)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            return new SupportReportResult { Success = false, Error = "Le titre et la description sont requis." };

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
            return new SupportReportResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send support report");
            return new SupportReportResult { Success = false, Error = "Erreur lors de l'envoi du signalement." };
        }
    }
}
