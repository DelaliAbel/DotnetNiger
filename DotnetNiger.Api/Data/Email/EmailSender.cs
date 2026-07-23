using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DotnetNiger.Api.Data.Email;

/// <summary>
/// Classe de base pour l'envoi d'emails via SMTP avec template HTML intégré.
/// </summary>
public class EmailSenderBase
{
    protected readonly SmtpOptions _smtp;
    protected readonly ILogger<EmailSenderBase> _logger;

    /// <summary>
    /// Initialise une nouvelle instance avec les options SMTP et le logger.
    /// </summary>
    public EmailSenderBase(IOptions<SmtpOptions> smtp, ILogger<EmailSenderBase> logger)
    {
        _smtp = smtp.Value;
        _logger = logger;
    }

    /// <summary>
    /// Construit le template HTML complet avec en-tête, pied de page et bouton CTA optionnel.
    /// </summary>
    protected string BuildTemplate(string title, string bodyHtml, string? ctaUrl = null, string? ctaText = null)
    {
        var ctaBlock = ctaUrl != null && ctaText != null
            ? $@"<p style=""text-align:center;margin:24px 0"">
  <a href=""{ctaUrl}"" style=""display:inline-block;padding:12px 28px;background:#0067b8;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px"">{ctaText}</a>
</p>"
            : "";

        return $@"<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""margin:0;padding:0;background-color:#f0f0f0;font-family:'Segoe UI',-apple-system,BlinkMacSystemFont,Roboto,sans-serif"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f0f0f0;padding:30px 10px"">
    <tr><td align=""center"">
      <table width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:8px;box-shadow:0 2px 12px rgba(0,0,0,0.08);overflow:hidden"">
        <tr>
          <td style=""padding:28px 36px 20px;background:#0067b8"">
            <h1 style=""color:#ffffff;margin:0;font-size:22px;font-weight:600;letter-spacing:0.3px"">{_smtp.AppName}</h1>
            {(string.IsNullOrEmpty(_smtp.AppSubtitle) ? "" : $"<p style=\"color:rgba(255,255,255,0.85);margin:4px 0 0;font-size:13px\">{_smtp.AppSubtitle}</p>")}
          </td>
        </tr>
        <tr><td style=""padding:28px 36px;color:#333333;font-size:15px;line-height:1.6"">
          <h2 style=""margin:0 0 16px;font-size:18px;color:#1a1a1a;font-weight:600"">{title}</h2>
          {bodyHtml}
        </td></tr>
        <tr>
          <td style=""padding:16px 36px;border-top:1px solid #e8e8e8;font-size:12px;color:#999999;text-align:center;line-height:1.5"">
            {_smtp.AppName} &mdash; &copy; 2026<br/>
            {(string.IsNullOrEmpty(_smtp.SupportEmail) ? "" : $"<a href=\"mailto:{_smtp.SupportEmail}\" style=\"color:#0067b8;text-decoration:none\">{_smtp.SupportEmail}</a>")}
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
    }

    /// <summary>
    /// Envoie un email à un seul destinataire via SMTP.
    /// </summary>
    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? replyTo = null)
    {
        if (string.IsNullOrEmpty(_smtp.Host))
        {
            _logger.LogInformation("[EMAIL] To={To} | Subject={Subject} | Body={Body}", toEmail, subject, htmlBody);
            return;
        }

        var message = BuildMessage(toEmail, subject, htmlBody, replyTo);
        try
        {
            await SendViaSmtpAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMTP indisponible, email non envoye. To={To} | Subject={Subject}", toEmail, subject);
        }
    }

    /// <summary>
    /// Envoie un email en masse à plusieurs destinataires.
    /// </summary>
    public async Task SendBatchAsync(string[] toEmails, string subject, string htmlBody, string? replyTo = null)
    {
        if (toEmails.Length == 0) return;

        if (string.IsNullOrEmpty(_smtp.Host))
        {
            _logger.LogInformation("[EMAIL] Batch a {Count} destinataires | Subject={Subject}", toEmails.Length, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
        message.Subject = subject;

        foreach (var email in toEmails)
            message.To.Add(MailboxAddress.Parse(email));

        if (!string.IsNullOrEmpty(replyTo))
            message.ReplyTo.Add(MailboxAddress.Parse(replyTo));

        var body = new TextPart("html") { Text = htmlBody };
        message.Body = body;

        await SendViaSmtpAsync(message);
    }

    private MimeMessage BuildMessage(string toEmail, string subject, string htmlBody, string? replyTo)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        if (!string.IsNullOrEmpty(replyTo))
            message.ReplyTo.Add(MailboxAddress.Parse(replyTo));

        message.MessageId = $"<{Guid.NewGuid():N}@{_smtp.FromEmail.Split('@').LastOrDefault() ?? "dotnetniger.com"}>";

        var plainText = StripHtml(htmlBody);

        var alternative = new MultipartAlternative { new TextPart("plain") { Text = plainText } };

        var related = new MultipartRelated();
        var htmlPart = new TextPart("html") { Text = htmlBody };
        related.Add(htmlPart);

        alternative.Add(related);
        message.Body = alternative;

        return message;
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return "";

        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", " ");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
        return text.Trim();
    }

    private async Task SendViaSmtpAsync(MimeMessage message)
    {
        using var client = new SmtpClient();

        if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase))
            client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

        await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.StartTlsWhenAvailable);
        if (!string.IsNullOrEmpty(_smtp.Username))
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Email sent to {Count} recipient(s)", message.To.Count);
    }
}
