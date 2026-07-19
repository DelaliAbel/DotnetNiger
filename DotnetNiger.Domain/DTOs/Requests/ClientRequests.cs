using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public record CreateOAuthClientRequest(
    [Required][StringLength(100, MinimumLength = 1)] string ClientName,
    string? Description,
    string? RedirectUris,
    string? PostLogoutRedirectUris,
    string? AllowedGrantTypes);

public record UpdateOAuthClientRequest(
    string? ClientName,
    string? Description,
    string? RedirectUris,
    string? PostLogoutRedirectUris,
    string? AllowedGrantTypes,
    bool? IsActive);
