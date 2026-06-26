namespace DotnetNiger.Gateway.Services;

/// <summary>Service de récupération des métadonnées Open Graph pour les pages de contenu.</summary>
public interface IOpenGraphService
{
    Task<OGMetadata?> FetchMetadataAsync(string type, string slug);
}

/// <summary>Métadonnées Open Graph d'une page (titre, description, image).</summary>
public class OGMetadata
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
