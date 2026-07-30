namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'un tag.</summary>
public class TagResponse
{
    /// <summary>Identifiant du tag.</summary>
    public Guid Id { get; set; }
    /// <summary>Nom du tag.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Slug URL du tag.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Nombre d'utilisations du tag.</summary>
    public int UsageCount { get; set; }
}
