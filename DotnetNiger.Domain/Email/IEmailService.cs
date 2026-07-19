namespace DotnetNiger.Domain.Email;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? replyTo = null);
    Task SendBatchAsync(string[] toEmails, string subject, string htmlBody, string? replyTo = null);
}
