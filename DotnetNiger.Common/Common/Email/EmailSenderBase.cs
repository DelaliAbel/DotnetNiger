using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DotnetNiger.Common.Email;

/// <summary>Base commune pour l'envoi d'emails avec template HTML.</summary>
public class EmailSenderBase
{
  /// <summary>Options SMTP.</summary>
  protected readonly SmtpOptions _smtp;

  /// <summary>Logger.</summary>
  protected readonly ILogger<EmailSenderBase> _logger;

  /// <summary>Initialise le service d'email avec les options SMTP.</summary>
  public EmailSenderBase(IOptions<SmtpOptions> smtp, ILogger<EmailSenderBase> logger)
  {
    _smtp = smtp.Value;
    _logger = logger;
  }

  /// <summary>Construit le template HTML complet avec header et footer.</summary>
  protected string BuildTemplate(string title, string bodyHtml, string? ctaUrl = null, string? ctaText = null)
  {
    var ctaBlock = ctaUrl != null && ctaText != null
        ? $@"<p style=""text-align:center;margin:24px 0"">
  <a href=""{ctaUrl}"" style=""display:inline-block;padding:12px 28px;background:#512BD4;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600"">{ctaText}</a>
</p>"
        : "";

    return $@"<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""margin:0;padding:0;background-color:#f2f2f2;font-family:'Segoe UI',Tahoma,Arial,sans-serif"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f2f2f2;padding:30px 0"">
    <tr><td align=""center"">
      <table width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.08);overflow:hidden"">
        <tr>
          <td style=""padding:32px 40px 24px;background:linear-gradient(135deg,#512BD4,#6b3ff5)"">
            <h1 style=""color:#ffffff;margin:0;font-size:22px;font-weight:600;letter-spacing:0.5px"">{_smtp.AppName}</h1>
            {(string.IsNullOrEmpty(_smtp.AppSubtitle) ? "" : $"<p style=\"color:rgba(255,255,255,0.8);margin:4px 0 0;font-size:13px\">{_smtp.AppSubtitle}</p>")}
          </td>
        </tr>
        <tr><td style=""padding:32px 40px;color:#333333"">
          <h2 style=""margin:0 0 16px;font-size:20px;color:#512BD4"">{title}</h2>
          {bodyHtml}
        </td></tr>
        <tr>
          <td style=""padding:16px 40px;border-top:1px solid #e8e8e8;font-size:12px;color:#999999;text-align:center"">
            {_smtp.AppName} &mdash; &copy; 2026
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
  }

  /// <summary>Envoie un email à un destinataire.</summary>
  public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? replyTo = null)
  {
    if (string.IsNullOrEmpty(_smtp.Host))
    {
      _logger.LogInformation("[EMAIL] To={To} | Subject={Subject} | Body={Body}", toEmail, subject, htmlBody);
      return;
    }

    using var message = BuildMessage(toEmail, subject, htmlBody, replyTo);
    await SendViaSmtpAsync(message);
  }

  /// <summary>Envoie un email à plusieurs destinataires (newsletter).</summary>
  public async Task SendBatchAsync(string[] toEmails, string subject, string htmlBody, string? replyTo = null)
  {
    if (toEmails.Length == 0) return;

    if (string.IsNullOrEmpty(_smtp.Host))
    {
      _logger.LogInformation("[EMAIL] Batch à {Count} destinataires | Subject={Subject}", toEmails.Length, subject);
      return;
    }

    using var message = new MimeMessage();
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

    var body = new TextPart("html") { Text = htmlBody };
    message.Body = body;
    return message;
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
