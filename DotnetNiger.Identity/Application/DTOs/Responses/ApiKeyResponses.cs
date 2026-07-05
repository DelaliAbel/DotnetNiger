namespace DotnetNiger.Identity.Application.DTOs.Responses;

/// <summary>Réponse contenant les données d'une clé API.</summary>
public record TenantApiKeyResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string KeyPrefix,
    string PublicKey,
    List<string> Scopes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? LastUsedAt);

/// <summary>Retourné une seule fois lors de la création de la clé.</summary>
public record TenantApiKeyCreatedResponse(
    TenantApiKeyResponse Key,
    string PrivateKey);
