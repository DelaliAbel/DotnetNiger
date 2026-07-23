namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Résultat de recherche unifié.</summary>
public class SearchResultResponse
{
    /// <summary>Type de contenu trouvé (post, event, resource, etc.).</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Identifiant du résultat.</summary>
    public Guid Id { get; set; }
    /// <summary>Titre du résultat.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Slug URL du résultat.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Extrait du contenu.</summary>
    public string? Excerpt { get; set; }
    /// <summary>Description du résultat.</summary>
    public string? Description { get; set; }
    /// <summary>Contenu partiel du résultat.</summary>
    public string? Content { get; set; }
    /// <summary>URL de l'image de couverture.</summary>
    public string? CoverImageUrl { get; set; }
    /// <summary>Date de début (pour les événements).</summary>
    public DateTime? StartDateTime { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; }
}
