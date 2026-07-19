namespace DotnetNiger.Common.DTOs.Responses;

/// <summary>Réponse contenant les données d'un utilisateur.</summary>
public record UserResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    Guid TenantId,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedAt,
    IList<string> Roles);

/// <summary>Réponse contenant le profil simplifié d'un utilisateur.</summary>
public record UserProfileResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    Guid? TenantId,
    IList<string> Roles);
