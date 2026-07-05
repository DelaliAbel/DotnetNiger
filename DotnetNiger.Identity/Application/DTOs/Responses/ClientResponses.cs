namespace DotnetNiger.Identity.Application.DTOs.Responses;

/// <summary>Réponse contenant les données d'un client OIDC.</summary>
public record TenantClientResponse(
    Guid Id,
    Guid TenantId,
    string ClientId,
    string ClientName,
    string? Description,
    List<string> RedirectUris,
    List<string> PostLogoutRedirectUris,
    List<string> AllowedGrantTypes,
    bool IsActive,
    DateTime CreatedAt);

/// <summary>Retourné une seule fois lors de la création du client.</summary>
public record TenantClientCreatedResponse(
    TenantClientResponse Client,
    string? ClientSecret);
