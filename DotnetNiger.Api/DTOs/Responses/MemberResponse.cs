namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse d'un membre de la communauté.</summary>
public record MemberResponse(
    // <summary>Identifiant du membre.</summary>
    Guid Id,
    // <summary>Identifiant de l'utilisateur associé.</summary>
    Guid UserId,
    // <summary>Nom d'affichage du membre.</summary>
    string DisplayName,
    // <summary>Biographie du membre.</summary>
    string? Bio,
    // <summary>Localisation du membre.</summary>
    string? Location,
    // <summary>URL du site web du membre.</summary>
    string? WebsiteUrl,
    // <summary>Date de création du profil.</summary>
    DateTime CreatedAt,
    // <summary>Date de dernière mise à jour.</summary>
    DateTime? UpdatedAt);
