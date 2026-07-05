using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using DotnetNiger.Common.Email;
using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Infrastructure;

/// <summary>Service d'envoi d'email implémentant IEmailSender pour Identity et IEmailService.</summary>
public class EmailSender : EmailSenderBase, IEmailSender<ApplicationUser>, IEmailService
{
    public EmailSender(IOptions<SmtpOptions> smtp, ILogger<EmailSender> logger)
        : base(smtp, logger) { }

    Task IEmailSender<ApplicationUser>.SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => SendConfirmationLinkAsync(user, email, confirmationLink);

    /// <summary>Email de confirmation d'inscription avec lien.</summary>
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink, string? tenantName = null)
    {
        var displayName = !string.IsNullOrEmpty(tenantName) ? $"{_smtp.AppName} — {tenantName}" : _smtp.AppName;
        return SendEmailAsync(email, $"Confirmez votre adresse email — {_smtp.AppName}",
            BuildTemplate(
                $"Bienvenue sur {displayName}",
                $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Merci de vous être inscrit sur <strong>{displayName}</strong>. Veuillez confirmer votre adresse email pour activer votre compte.</p>
<p style=""text-align:center;margin:24px 0"">
  <a href=""{confirmationLink}"" style=""display:inline-block;padding:12px 28px;background:#512BD4;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600"">Confirmer mon email</a>
</p>
<p style=""font-size:13px;color:#666"">Si le bouton ne fonctionne pas, copiez ce lien dans votre navigateur&nbsp;:</p>
<p style=""font-size:12px;color:#999;word-break:break-all"">{confirmationLink}</p>"));
    }

    /// <summary>Email de réinitialisation de mot de passe.</summary>
    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        return SendEmailAsync(email, $"Réinitialisation de mot de passe — {_smtp.AppName}",
            BuildTemplate(
                "Réinitialisation de mot de passe",
                $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Vous avez demandé la réinitialisation de votre mot de passe.</p>
<p style=""text-align:center;margin:24px 0"">
  <a href=""{resetLink}"" style=""display:inline-block;padding:12px 28px;background:#512BD4;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600"">Réinitialiser mon mot de passe</a>
</p>
<p style=""font-size:13px;color:#666"">Si vous n'êtes pas à l'origine de cette demande, ignorez cet email.</p>"));
    }

    /// <summary>Email avec code de réinitialisation.</summary>
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        return SendEmailAsync(email, $"Code de réinitialisation — {_smtp.AppName}",
            BuildTemplate(
                "Code de réinitialisation",
                $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Voici votre code de réinitialisation de mot de passe&nbsp;:</p>
<p style=""text-align:center;margin:24px 0;font-size:32px;font-weight:700;letter-spacing:8px;color:#512BD4;font-family:'Courier New',monospace"">{resetCode}</p>
<p style=""font-size:13px;color:#666"">Ce code expire dans 15 minutes.</p>"));
    }

    /// <summary>Email d'invitation pour un administrateur.</summary>
    public Task SendInviteEmailAsync(string email, string inviteUrl, string role)
    {
        return SendEmailAsync(email, $"Vous avez été invité sur {_smtp.AppName}",
            BuildTemplate(
                "Invitation à rejoindre",
                $@"<p>Bonjour,</p>
<p>Vous avez été invité à rejoindre {_smtp.AppName} en tant que <strong>{role}</strong>.</p>
<p style=""text-align:center;margin:24px 0"">
  <a href=""{inviteUrl}"" style=""display:inline-block;padding:12px 28px;background:#512BD4;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600"">Accepter l'invitation</a>
</p>
<p style=""font-size:13px;color:#666"">Cette invitation expire dans 48 heures.</p>"));
    }

    /// <summary>Email avec code de confirmation et lien optionnel.</summary>
    public Task SendConfirmationCodeAsync(ApplicationUser user, string email, string code, string? confirmationLink = null, string? tenantName = null)
    {
        var displayName = !string.IsNullOrEmpty(tenantName) ? $"{_smtp.AppName} — {tenantName}" : _smtp.AppName;
        var linkHtml = confirmationLink != null
            ? $@"<p style=""text-align:center;margin:24px 0"">
  <a href=""{confirmationLink}"" style=""display:inline-block;padding:12px 28px;background:#512BD4;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600"">Confirmer mon compte</a>
</p>"
            : "";

        return SendEmailAsync(email, $"Votre code de confirmation — {_smtp.AppName}",
            BuildTemplate(
                "Confirmez votre inscription",
                $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Utilisez le code ci-dessous pour activer votre compte sur <strong>{displayName}</strong>&nbsp;:</p>
<p style=""text-align:center;margin:24px 0;padding:16px;background:#f5f2ff;border-radius:8px"">
  <span style=""font-size:36px;font-weight:700;letter-spacing:10px;color:#512BD4;font-family:'Courier New',monospace"">{code}</span>
</p>
{linkHtml}
<p style=""font-size:13px;color:#666"">Ce code expire dans 15 minutes. Si vous n'avez pas créé de compte, ignorez cet email.</p>"));
    }
}
