namespace DotnetNiger.Common.Auth.Responses;

/// <summary>
/// Réponse après inscription d'un tenant.
/// </summary>
public record RegisterTenantResponse(
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    string AdminEmail,
    string ClientId,
    string ClientSecret,
    Guid ApiKeyId,
    string ApiKeySecret);
