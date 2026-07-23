using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de mise à jour d'une ressource.</summary>
public class UpdateResourceRequest
{
    /// <summary>Titre de la ressource (max 200 caractères).</summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>Slug URL de la ressource.</summary>
    public string? Slug { get; set; }

    /// <summary>Description de la ressource.</summary>
    public string? Description { get; set; }

    /// <summary>URL principale de la ressource.</summary>
    public string? Url { get; set; }

    /// <summary>URL de téléchargement.</summary>
    public string? DownloadUrl { get; set; }

    /// <summary>URL de l'aperçu visuel.</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>Type de ressource (vidéo, PDF, etc.).</summary>
    public string? ResourceType { get; set; }

    /// <summary>Niveau de difficulté.</summary>
    public string? Level { get; set; }

    /// <summary>Identifiants des catégories associées.</summary>
    public List<Guid>? CategoryIds { get; set; }

    /// <summary>Identifiants des tags associés.</summary>
    public List<Guid>? TagIds { get; set; }

    /// <summary>Noms des tags associés.</summary>
    public List<string>? TagNames { get; set; }
}
