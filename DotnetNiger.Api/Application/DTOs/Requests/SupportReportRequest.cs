namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de signalement de problème de support.</summary>
public class SupportReportRequest
{
    /// <summary>Titre du signalement.</summary>
    public string Title { get; set; } = "";
    /// <summary>Description du problème.</summary>
    public string Description { get; set; } = "";
    /// <summary>Type de problème (bug, suggestion, etc.).</summary>
    public string? Type { get; set; }
    /// <summary>Étapes pour reproduire le problème.</summary>
    public string? Steps { get; set; }
    /// <summary>URL de la page concernée.</summary>
    public string? PageUrl { get; set; }
    /// <summary>User-Agent du navigateur.</summary>
    public string? UserAgent { get; set; }
}
