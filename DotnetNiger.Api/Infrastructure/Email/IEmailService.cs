namespace DotnetNiger.Api.Infrastructure.Email;

/// <summary>
/// Interface du service d'envoi d'emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envoie un email à un seul destinataire.
    /// </summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? replyTo = null);

    /// <summary>
    /// Envoie un email à plusieurs destinataires.
    /// </summary>
    Task SendBatchAsync(string[] toEmails, string subject, string htmlBody, string? replyTo = null);
}
