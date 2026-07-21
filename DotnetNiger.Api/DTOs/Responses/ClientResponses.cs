namespace DotnetNiger.Api.DTOs.Responses;

public record OAuthClientResponse(
    Guid Id,
    string ClientId,
    string ClientName,
    string? Description,
    List<string> RedirectUris,
    List<string> PostLogoutRedirectUris,
    List<string> AllowedGrantTypes,
    bool IsActive,
    DateTime CreatedAt);

public record OAuthClientCreatedResponse(
    OAuthClientResponse Client,
    string? ClientSecret);
