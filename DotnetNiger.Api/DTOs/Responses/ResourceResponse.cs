namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse d'une ressource éducative.</summary>
public record ResourceResponse(
    // <summary>Identifiant de la ressource.</summary>
    Guid Id,
    // <summary>Titre de la ressource.</summary>
    string Title,
    // <summary>Slug URL de la ressource.</summary>
    string Slug,
    // <summary>Description de la ressource.</summary>
    string Description,
    // <summary>URL principale de la ressource.</summary>
    string Url,
    // <summary>URL de téléchargement.</summary>
    string? DownloadUrl,
    // <summary>URL de l'aperçu visuel.</summary>
    string? ThumbnailUrl,
    // <summary>Identifiant de l'auteur.</summary>
    Guid AuthorId,
    // <summary>Statut de la ressource.</summary>
    string Status,
    // <summary>Date de création.</summary>
    DateTime CreatedAt,
    // <summary>Date de dernière mise à jour.</summary>
    DateTime UpdatedAt);
