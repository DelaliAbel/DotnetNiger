using DotnetNiger.Api.Infrastructure.Email;
using DotnetNiger.Api.Domain.Entities;

namespace DotnetNiger.Api.Infrastructure.Email.Templates;

/// <summary>
/// Template HTML pour l'email de confirmation par lien.
/// </summary>
public static class ConfirmationLinkTemplate
{
    /// <summary>
    /// Génère le subject, titre et corps HTML du lien de confirmation.
    /// </summary>
    public static (string subject, string title, string body) Render(ApplicationUser user, string confirmationLink, SmtpOptions smtp)
    {
        return (
            $"Confirmez votre adresse email — {smtp.AppName}",
            $"Bienvenue sur {smtp.AppName}",
            $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Merci de vous etre inscrit sur <strong>{smtp.AppName}</strong>. Veuillez confirmer votre adresse email pour activer votre compte.</p>
<p style=""text-align:center;margin:24px 0"">
  <a href=""{confirmationLink}"" style=""display:inline-block;padding:12px 28px;background:#0067b8;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px"">Confirmer mon email</a>
</p>
<p style=""font-size:13px;color:#666"">Si le bouton ne fonctionne pas, copiez ce lien dans votre navigateur :</p>
<p style=""font-size:12px;color:#999;word-break:break-all"">{confirmationLink}</p>"
        );
    }
}
