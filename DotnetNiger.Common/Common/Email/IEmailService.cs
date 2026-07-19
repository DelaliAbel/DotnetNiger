namespace DotnetNiger.Common.Email;

/// <summary>Interface commune d'envoi d'emails (simple, batch, personnalisé).</summary>
public interface IEmailService
{
    /// <summary>Envoie un email à un destinataire.</summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? replyTo = null);

    /// <summary>Envoie un email à plusieurs destinataires (newsletter, notification).</summary>
    Task SendBatchAsync(string[] toEmails, string subject, string htmlBody, string? replyTo = null);
}
