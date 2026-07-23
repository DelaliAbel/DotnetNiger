namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse complète d'un article.</summary>
public record PostResponse(
    // <summary>Identifiant de l'article.</summary>
    Guid Id,
    // <summary>Titre de l'article.</summary>
    string Title,
    // <summary>Slug URL de l'article.</summary>
    string Slug,
    // <summary>Contenu de l'article.</summary>
    string Content,
    // <summary>Extrait de l'article.</summary>
    string Excerpt,
    // <summary>URL de l'image de couverture.</summary>
    string CoverImageUrl,
    // <summary>Identifiant de l'auteur.</summary>
    Guid AuthorId,
    // <summary>Statut de l'article (draft, published, etc.).</summary>
    string Status,
    // <summary>Date de publication.</summary>
    DateTime? PublishedAt,
    // <summary>Date de création.</summary>
    DateTime CreatedAt,
    // <summary>Date de dernière mise à jour.</summary>
    DateTime UpdatedAt);
