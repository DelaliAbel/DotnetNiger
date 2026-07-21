using DotnetNiger.Api.Data.Email;
using DotnetNiger.Api.Entities;

namespace DotnetNiger.Api.Data.Email.Templates;

public static class ConfirmationCodeTemplate
{
    public static (string subject, string title, string body) Render(ApplicationUser user, string code, SmtpOptions smtp, string? confirmationLink = null)
    {
        var linkHtml = confirmationLink != null
            ? $@"<p style=""text-align:center;margin:24px 0"">
  <a href=""{confirmationLink}"" style=""display:inline-block;padding:12px 28px;background:#0067b8;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px"">Confirmer mon compte</a>
</p>"
            : "";

        return (
            $"Votre code de confirmation — {smtp.AppName}",
            "Confirmez votre inscription",
            $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Utilisez le code ci-dessous pour activer votre compte sur <strong>{smtp.AppName}</strong> :</p>
<p style=""text-align:center;margin:24px 0;padding:16px;background:#f5f5f5;border-radius:8px"">
  <span style=""font-size:36px;font-weight:700;letter-spacing:10px;color:#0067b8;font-family:'Courier New',monospace"">{code}</span>
</p>
{linkHtml}
<p style=""font-size:13px;color:#666"">Ce code expire dans 15 minutes. Si vous n'avez pas cree de compte, ignorez cet email.</p>"
        );
    }
}
