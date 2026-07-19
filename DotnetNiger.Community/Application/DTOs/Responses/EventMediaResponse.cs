namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>Réponse contenant les données d'un média d'événement.</summary>
public class EventMediaResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
