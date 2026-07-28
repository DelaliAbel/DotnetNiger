namespace DotnetNiger.Api.Infrastructure.Email.Templates;

/// <summary>Template HTML pour l'email de notification admin d'un nouveau message de contact.</summary>
public static class ContactNotificationTemplate
{
    public static (string subject, string body) Render(string fullName, string email, string subject, string message)
    {
        return (
            $"[Contact] {subject}",
            $@"<h3>Nouveau message de contact</h3>
<table style='border-collapse:collapse;width:100%'>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Nom</td><td style='padding:8px;border:1px solid #ddd'>{fullName}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Email</td><td style='padding:8px;border:1px solid #ddd'>{email}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Objet</td><td style='padding:8px;border:1px solid #ddd'>{subject}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Message</td><td style='padding:8px;border:1px solid #ddd'>{message}</td></tr>
</table>"
        );
    }
}
