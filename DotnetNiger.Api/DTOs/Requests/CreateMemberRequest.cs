namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de création d'un membre.</summary>
public record CreateMemberRequest(
    // <summary>Nom d'affichage du membre.</summary>
    string DisplayName,
    // <summary>Biographie du membre.</summary>
    string? Bio = null,
    // <summary>Localisation du membre.</summary>
    string? Location = null,
    // <summary>URL du site web du membre.</summary>
    string? WebsiteUrl = null);
