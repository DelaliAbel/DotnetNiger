namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de signalement d'un problème par un utilisateur.</summary>
public class SupportReportRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Type { get; set; }
    public string? Steps { get; set; }
    public string? PageUrl { get; set; }
    public string? UserAgent { get; set; }
}
