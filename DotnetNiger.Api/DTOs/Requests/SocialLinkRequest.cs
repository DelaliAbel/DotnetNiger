namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de lien social (ancien format).</summary>
public record SocialLinkRequest(
    // <summary>Nom de la plateforme (GitHub, LinkedIn, etc.).</summary>
    string Platform,
    // <summary>URL du profil social.</summary>
    string Url);
