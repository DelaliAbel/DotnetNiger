using DotnetNiger.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Api.Data.Email;

/// <summary>
/// Implémentation concrète du service d'envoi d'emails avec les templates spécifiques.
/// </summary>
public class EmailSender : EmailSenderBase, IEmailSender<ApplicationUser>, IEmailService
{
    /// <summary>
    /// Initialise une nouvelle instance du service d'envoi d'emails.
    /// </summary>
    public EmailSender(IOptions<SmtpOptions> smtp, ILogger<EmailSenderBase> logger)
        : base(smtp, logger) { }

    Task IEmailSender<ApplicationUser>.SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => SendConfirmationLinkAsync(user, email, confirmationLink);

    /// <summary>
    /// Envoie un lien de confirmation d'inscription à l'utilisateur.
    /// </summary>
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var (subject, title, body) = ConfirmationLinkTemplate.Render(user, confirmationLink, _smtp);
        return SendEmailAsync(email, subject, BuildTemplate(title, body));
    }

    /// <summary>
    /// Envoie un lien de réinitialisation de mot de passe.
    /// </summary>
    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var (subject, title, body) = PasswordResetLinkTemplate.Render(user, resetLink, _smtp);
        return SendEmailAsync(email, subject, BuildTemplate(title, body));
    }

    /// <summary>
    /// Envoie un code de réinitialisation de mot de passe.
    /// </summary>
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var (subject, title, body) = PasswordResetCodeTemplate.Render(user, resetCode, _smtp);
        return SendEmailAsync(email, subject, BuildTemplate(title, body));
    }

    /// <summary>
    /// Envoie un email d'invitation à rejoindre l'application.
    /// </summary>
    public Task SendInviteEmailAsync(string email, string inviteUrl, string role)
    {
        var (subject, title, body) = InviteEmailTemplate.Render(inviteUrl, role, _smtp);
        return SendEmailAsync(email, subject, BuildTemplate(title, body));
    }

    /// <summary>
    /// Envoie un code de confirmation d'inscription à l'utilisateur.
    /// </summary>
    public Task SendConfirmationCodeAsync(ApplicationUser user, string email, string code, string? confirmationLink = null)
    {
        var (subject, title, body) = ConfirmationCodeTemplate.Render(user, code, _smtp, confirmationLink);
        return SendEmailAsync(email, subject, BuildTemplate(title, body));
    }
}
