namespace DotnetNiger.Common.Auth.Responses;

/// <summary>
/// Réponse contenant les informations d'un utilisateur.
/// </summary>
public record UserInfoResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    Guid? TenantId,
    bool IsActive,
    IList<string> Roles,
    IList<string> Permissions,
    bool RememberMe = false);
