namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Métadonnées Open Graph pour le partage sur les réseaux sociaux.</summary>
public class OGMetadata
{
    /// <summary>Titre de la page.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Description de la page.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>URL de l'image de partage.</summary>
    public string ImageUrl { get; set; } = string.Empty;
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime UpdatedAt { get; set; }
}
