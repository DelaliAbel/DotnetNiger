using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de mise à jour d'un article.</summary>
public class UpdatePostRequest
{
    /// <summary>Titre de l'article (max 200 caractères).</summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>Slug URL de l'article.</summary>
    public string? Slug { get; set; }

    /// <summary>Contenu de l'article.</summary>
    public string? Content { get; set; }

    /// <summary>Extrait de l'article (max 500 caractères).</summary>
    [MaxLength(500)]
    public string? Excerpt { get; set; }

    /// <summary>URL de l'image de couverture.</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>Type d'article.</summary>
    public string? PostType { get; set; }

    /// <summary>Identifiants des catégories associées.</summary>
    public List<Guid>? CategoryIds { get; set; }

    /// <summary>Noms des tags associés.</summary>
    public List<string>? TagNames { get; set; }

    /// <summary>Indique si l'article est publié.</summary>
    public bool? IsPublished { get; set; }
}
