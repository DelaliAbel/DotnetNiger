using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de création d'un article.</summary>
public class CreatePostRequest
{
    /// <summary>Titre de l'article (max 200 caractères).</summary>
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Slug URL de l'article.</summary>
    public string? Slug { get; set; }

    /// <summary>Contenu de l'article.</summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>Extrait de l'article (max 500 caractères).</summary>
    [MaxLength(500)]
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>URL de l'image de couverture.</summary>
    public string CoverImageUrl { get; set; } = string.Empty;

    /// <summary>Type d'article (article, tutoriel, etc.).</summary>
    [Required]
    public string PostType { get; set; } = string.Empty;

    /// <summary>Identifiants des catégories associées.</summary>
    public List<Guid> CategoryIds { get; set; } = [];
    /// <summary>Identifiants des tags associés.</summary>
    public List<Guid> TagIds { get; set; } = [];
    /// <summary>Noms des tags associés.</summary>
    public List<string> TagNames { get; set; } = [];
    /// <summary>Indique si l'article est publié.</summary>
    public bool IsPublished { get; set; }
}
