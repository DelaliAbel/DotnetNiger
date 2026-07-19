namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>Métadonnées Open Graph pour le partage social.</summary>
public class OGMetadata
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
