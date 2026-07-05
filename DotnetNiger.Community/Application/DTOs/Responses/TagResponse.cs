namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>Réponse contenant les données d'un tag.</summary>
public class TagResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}
