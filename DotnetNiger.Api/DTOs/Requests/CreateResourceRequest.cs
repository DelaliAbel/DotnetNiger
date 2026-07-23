using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de création d'une ressource.</summary>
public class CreateResourceRequest
{
    /// <summary>Titre de la ressource (max 200 caractères).</summary>
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Slug URL de la ressource.</summary>
    public string? Slug { get; set; }

    /// <summary>Description de la ressource.</summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>URL principale de la ressource.</summary>
    [Required, Url]
    public string Url { get; set; } = string.Empty;

    /// <summary>URL de téléchargement.</summary>
    public string? DownloadUrl { get; set; }

    /// <summary>URL de l'aperçu visuel.</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>Type de ressource.</summary>
    [Required]
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>Niveau de difficulté.</summary>
    [Required]
    public string Level { get; set; } = string.Empty;

    /// <summary>Identifiants des catégories associées.</summary>
    public List<Guid> CategoryIds { get; set; } = [];
    /// <summary>Identifiants des tags associés.</summary>
    public List<Guid> TagIds { get; set; } = [];
    /// <summary>Noms des tags associés.</summary>
    public List<string> TagNames { get; set; } = [];
}
