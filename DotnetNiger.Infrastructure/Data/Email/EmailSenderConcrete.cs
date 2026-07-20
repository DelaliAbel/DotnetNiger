using DotnetNiger.Domain.Email;
using DotnetNiger.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Infrastructure.Data.Email;

public class EmailSender : EmailSenderBase, IEmailSender<ApplicationUser>, IEmailService
{
    public EmailSender(IOptions<SmtpOptions> smtp, ILogger<EmailSenderBase> logger)
        : base(smtp, logger) { }

    Task IEmailSender<ApplicationUser>.SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => SendConfirmationLinkAsync(user, email, confirmationLink);

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var (subject, title, body) = ConfirmationLinkTemplate.Render(user, confirmationLink, _smtp);
        return SendEmailAsync(email, subject, BuildTemplate(title, body));
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var (subject, title, body) = PasswordResetLinkTemplate.Render(user, resetLink, _smtp);
        return SendEmailAsync(email, subject, BuildTemplate(title, body));
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var (subject, title, body) = PasswordResetCodeTemplate.Render(user, resetCode, _smtp);
        return SendEmailAsync(email, subject, BuildTemplate(title, body));
    }

    public Task SendInviteEmailAsync(string email, string inviteUrl, string role)
    {
        var (subject, title, body) = InviteEmailTemplate.Render(inviteUrl, role, _smtp);
        return SendEmailAsync(email, subject, BuildTemplate(title, body));
    }

    public Task SendConfirmationCodeAsync(ApplicationUser user, string email, string code, string? confirmationLink = null)
    {
        var (subject, title, body) = ConfirmationCodeTemplate.Render(user, code, _smtp, confirmationLink);
        return SendEmailAsync(email, subject, BuildTemplate(title, body));
    }
}
