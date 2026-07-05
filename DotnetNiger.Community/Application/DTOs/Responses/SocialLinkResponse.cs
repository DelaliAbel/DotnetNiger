namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>Réponse contenant les données d'un lien social.</summary>
public class SocialLinkResponse
{
    public Guid Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
