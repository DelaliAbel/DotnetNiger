namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'un lien social du profil.</summary>
public class SocialLinkResponse
{
    /// <summary>Identifiant du lien social.</summary>
    public Guid Id { get; set; }
    /// <summary>Nom de la plateforme (GitHub, LinkedIn, etc.).</summary>
    public string Platform { get; set; } = string.Empty;
    /// <summary>URL du profil social.</summary>
    public string Url { get; set; } = string.Empty;
}
