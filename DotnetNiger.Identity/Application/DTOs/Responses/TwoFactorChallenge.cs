namespace DotnetNiger.Identity.Application.DTOs.Responses;

/// <summary>Défi d'authentification à deux facteurs (cache interne).</summary>
public record TwoFactorChallenge(
    Guid UserId,
    string Email,
    Guid TenantId,
    DateTime ExpiresAt);
