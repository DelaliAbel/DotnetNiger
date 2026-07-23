namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse détaillée des informations utilisateur (pour le token).</summary>
public record UserInfoResponse(
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
    // <summary>Rôles attribués.</summary>
    IList<string> Roles,
    // <summary>Permissions attribuées.</summary>
    IList<string> Permissions,
    // <summary>Indique si la session doit être maintenue.</summary>
    bool RememberMe = false);
