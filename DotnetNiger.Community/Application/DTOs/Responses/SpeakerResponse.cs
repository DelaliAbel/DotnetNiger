namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>Réponse contenant les données d'un intervenant.</summary>
public class SpeakerResponse
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}
