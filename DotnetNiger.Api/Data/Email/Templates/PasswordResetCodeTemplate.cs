using DotnetNiger.Api.Data.Email;
using DotnetNiger.Api.Entities;

namespace DotnetNiger.Api.Data.Email.Templates;

public static class PasswordResetCodeTemplate
{
    public static (string subject, string title, string body) Render(ApplicationUser user, string resetCode, SmtpOptions smtp)
    {
        return (
            $"Code de reinitialisation — {smtp.AppName}",
            "Code de reinitialisation",
            $@"<p>Bonjour {user.FirstName ?? ""},</p>
<p>Voici votre code de reinitialisation de mot de passe :</p>
<p style=""text-align:center;margin:24px 0;padding:16px;background:#f5f5f5;border-radius:8px"">
  <span style=""font-size:36px;font-weight:700;letter-spacing:10px;color:#0067b8;font-family:'Courier New',monospace"">{resetCode}</span>
</p>
<p style=""font-size:13px;color:#666"">Ce code expire dans 15 minutes.</p>"
        );
    }
}
