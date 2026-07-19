using DotnetNiger.Common.Email;
using DotnetNiger.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Infrastructure;

public class EmailSender : EmailSenderBase, IEmailSender<ApplicationUser>, IEmailService
{
    public EmailSender(IOptions<SmtpOptions> smtp, ILogger<EmailSenderBase> logger)
        : base(smtp, logger) { }

    Task IEmailSender<ApplicationUser>.SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => SendConfirmationLinkAsync(user, email, confirmationLink);

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink, string? tenantName = null)
    {
        var displayName = !string.IsNullOrEmpty(tenantName) ? $"{_smtp.AppName} — {tenantName}" : _smtp.AppName;
        return SendEmailAsync(email, $"Confirmez votre adresse email — {_smtp.AppName}",
            BuildTemplate(
                $"Bienvenue sur {displayName}",
                $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Merci de vous etre inscrit sur <strong>{displayName}</strong>. Veuillez confirmer votre adresse email pour activer votre compte.</p>
<p style=""text-align:center;margin:24px 0"">
  <a href=""{confirmationLink}"" style=""display:inline-block;padding:12px 28px;background:#0067b8;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px"">Confirmer mon email</a>
</p>
<p style=""font-size:13px;color:#666"">Si le bouton ne fonctionne pas, copiez ce lien dans votre navigateur :</p>
<p style=""font-size:12px;color:#999;word-break:break-all"">{confirmationLink}</p>"));
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        return SendEmailAsync(email, $"Reinitialisation de mot de passe — {_smtp.AppName}",
            BuildTemplate(
                "Reinitialisation de mot de passe",
                $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Vous avez demande la reinitialisation de votre mot de passe.</p>
<p style=""text-align:center;margin:24px 0"">
  <a href=""{resetLink}"" style=""display:inline-block;padding:12px 28px;background:#0067b8;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px"">Reinitialiser mon mot de passe</a>
</p>
<p style=""font-size:13px;color:#666"">Si vous n'etes pas a l'origine de cette demande, ignorez cet email.</p>"));
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        return SendEmailAsync(email, $"Code de reinitialisation — {_smtp.AppName}",
            BuildTemplate(
                "Code de reinitialisation",
                $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Voici votre code de reinitialisation de mot de passe :</p>
<p style=""text-align:center;margin:24px 0;padding:16px;background:#f5f5f5;border-radius:8px"">
  <span style=""font-size:36px;font-weight:700;letter-spacing:10px;color:#0067b8;font-family:'Courier New',monospace"">{resetCode}</span>
</p>
<p style=""font-size:13px;color:#666"">Ce code expire dans 15 minutes.</p>"));
    }

    public Task SendInviteEmailAsync(string email, string inviteUrl, string role)
    {
        return SendEmailAsync(email, $"Vous avez ete invite sur {_smtp.AppName}",
            BuildTemplate(
                "Invitation a rejoindre",
                $@"<p>Bonjour,</p>
<p>Vous avez ete invite a rejoindre {_smtp.AppName} en tant que <strong>{role}</strong>.</p>
<p style=""text-align:center;margin:24px 0"">
  <a href=""{inviteUrl}"" style=""display:inline-block;padding:12px 28px;background:#0067b8;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px"">Accepter l'invitation</a>
</p>
<p style=""font-size:13px;color:#666"">Cette invitation expire dans 48 heures.</p>"));
    }

    public Task SendConfirmationCodeAsync(ApplicationUser user, string email, string code, string? confirmationLink = null, string? tenantName = null)
    {
        var displayName = !string.IsNullOrEmpty(tenantName) ? $"{_smtp.AppName} — {tenantName}" : _smtp.AppName;
        var linkHtml = confirmationLink != null
            ? $@"<p style=""text-align:center;margin:24px 0"">
  <a href=""{confirmationLink}"" style=""display:inline-block;padding:12px 28px;background:#0067b8;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px"">Confirmer mon compte</a>
</p>"
            : "";

        return SendEmailAsync(email, $"Votre code de confirmation — {_smtp.AppName}",
            BuildTemplate(
                "Confirmez votre inscription",
                $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Utilisez le code ci-dessous pour activer votre compte sur <strong>{displayName}</strong> :</p>
<p style=""text-align:center;margin:24px 0;padding:16px;background:#f5f5f5;border-radius:8px"">
  <span style=""font-size:36px;font-weight:700;letter-spacing:10px;color:#0067b8;font-family:'Courier New',monospace"">{code}</span>
</p>
{linkHtml}
<p style=""font-size:13px;color:#666"">Ce code expire dans 15 minutes. Si vous n'avez pas cree de compte, ignorez cet email.</p>"));
    }
}
