using DotnetNiger.Api.DTOs.Requests;

namespace DotnetNiger.Api.Data.Email.Templates;

public static class SupportReportTemplate
{
    public static (string subject, string body) Render(SupportReportRequest request, string userId, string userEmail)
    {
        return (
            $"[Signalement] {request.Title}",
            $@"<h3>Nouveau signalement de bug</h3>
<table style='border-collapse:collapse;width:100%'>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Titre</td><td style='padding:8px;border:1px solid #ddd'>{request.Title}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Type</td><td style='padding:8px;border:1px solid #ddd'>{request.Type ?? "bug"}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Description</td><td style='padding:8px;border:1px solid #ddd'>{request.Description}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>etapes pour reproduire</td><td style='padding:8px;border:1px solid #ddd'>{request.Steps ?? "Non fourni"}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Page URL</td><td style='padding:8px;border:1px solid #ddd'>{request.PageUrl ?? "Non fourni"}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Navigateur/OS</td><td style='padding:8px;border:1px solid #ddd'>{request.UserAgent ?? "Non fourni"}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Utilisateur ID</td><td style='padding:8px;border:1px solid #ddd'>{userId}</td></tr>
<tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold'>Email</td><td style='padding:8px;border:1px solid #ddd'>{userEmail}</td></tr>
</table>"
        );
    }
}
