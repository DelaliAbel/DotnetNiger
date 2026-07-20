using DotnetNiger.Domain.Email;

namespace DotnetNiger.Infrastructure.Data.Email.Templates;

public static class InviteEmailTemplate
{
    public static (string subject, string title, string body) Render(string inviteUrl, string role, SmtpOptions smtp)
    {
        return (
            $"Vous avez ete invite sur {smtp.AppName}",
            "Invitation a rejoindre",
            $@"<p>Bonjour,</p>
<p>Vous avez ete invite a rejoindre {smtp.AppName} en tant que <strong>{role}</strong>.</p>
<p style=""text-align:center;margin:24px 0"">
  <a href=""{inviteUrl}"" style=""display:inline-block;padding:12px 28px;background:#0067b8;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px"">Accepter l'invitation</a>
</p>
<p style=""font-size:13px;color:#666"">Cette invitation expire dans 48 heures.</p>"
        );
    }
}
