using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DotnetNiger.Domain.Email;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.General;

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

    public async Task<SupportReportResult> ReportAsync(SupportReportRequest request, string userId, string userEmail)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            return new SupportReportResult { Success = false, Error = "Le titre et la description sont requis." };

        var supportEmail = _smtp.SupportEmail;
        if (string.IsNullOrEmpty(supportEmail))
            supportEmail = "koffilevis21@gmail.com";

        try
        {
            var (subject, body) = SupportReportTemplate.Render(request, userId, userEmail);
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
