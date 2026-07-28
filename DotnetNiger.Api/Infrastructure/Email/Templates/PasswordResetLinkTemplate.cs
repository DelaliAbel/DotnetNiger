using DotnetNiger.Api.Infrastructure.Email;
using DotnetNiger.Api.Domain.Entities;

namespace DotnetNiger.Api.Infrastructure.Email.Templates;

/// <summary>
/// Template HTML pour l'email de lien de réinitialisation de mot de passe.
/// </summary>
public static class PasswordResetLinkTemplate
{
    /// <summary>
    /// Génère le subject, titre et corps HTML du lien de réinitialisation.
    /// </summary>
    public static (string subject, string title, string body) Render(ApplicationUser user, string resetLink, SmtpOptions smtp)
    {
        return (
            $"Reinitialisation de mot de passe — {smtp.AppName}",
            "Reinitialisation de mot de passe",
            $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Vous avez demande la reinitialisation de votre mot de passe.</p>
<p style=""text-align:center;margin:24px 0"">
  <a href=""{resetLink}"" style=""display:inline-block;padding:12px 28px;background:#0067b8;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px"">Reinitialiser mon mot de passe</a>
</p>
<p style=""font-size:13px;color:#666"">Si vous n'etes pas a l'origine de cette demande, ignorez cet email.</p>"
        );
    }
}
