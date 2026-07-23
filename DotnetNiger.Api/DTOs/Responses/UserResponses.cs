namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse complète d'un utilisateur.</summary>
public record UserResponse(
    // <summary>Identifiant de l'utilisateur.</summary>
    Guid Id,
    // <summary>Adresse e-mail.</summary>
    string Email,
    // <summary>Prénom.</summary>
    string? FirstName,
    // <summary>Nom de famille.</summary>
    string? LastName,
    // <summary>URL de l'avatar.</summary>
    string? AvatarUrl,
    // <summary>Indique si le compte est actif.</summary>
    bool IsActive,
    // <summary>Indique si l'e-mail est confirmé.</summary>
    bool EmailConfirmed,
    // <summary>Date de création du compte.</summary>
    DateTime CreatedAt,
    // <summary>Rôles attribués à l'utilisateur.</summary>
    IList<string> Roles);

/// <summary>Réponse simplifiée du profil utilisateur.</summary>
public record UserProfileResponse(
    // <summary>Identifiant de l'utilisateur.</summary>
    Guid Id,
    // <summary>Adresse e-mail.</summary>
    string Email,
    // <summary>Prénom.</summary>
    string? FirstName,
    // <summary>Nom de famille.</summary>
    string? LastName,
    // <summary>URL de l'avatar.</summary>
    string? AvatarUrl,
    // <summary>Rôles attribués.</summary>
    IList<string> Roles);
