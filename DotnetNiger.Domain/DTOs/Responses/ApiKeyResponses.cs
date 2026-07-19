namespace DotnetNiger.Domain.DTOs.Responses;

public record ApiKeyResponse(
    Guid Id,
    string Name,
    string KeyPrefix,
    string PublicKey,
    List<string> Scopes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? LastUsedAt);

public record ApiKeyCreatedResponse(
    ApiKeyResponse Key,
    string PrivateKey);
