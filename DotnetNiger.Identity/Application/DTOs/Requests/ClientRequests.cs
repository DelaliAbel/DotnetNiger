using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de création d'un client OIDC pour un tenant.</summary>
public record CreateTenantClientRequest(
    [Required][StringLength(100, MinimumLength = 1)] string ClientName,
    string? Description,
    string? RedirectUris,
    string? PostLogoutRedirectUris,
    string? AllowedGrantTypes);

/// <summary>Requête de mise à jour d'un client OIDC.</summary>
public record UpdateTenantClientRequest(
    string? ClientName,
    string? Description,
    string? RedirectUris,
    string? PostLogoutRedirectUris,
    string? AllowedGrantTypes,
    bool? IsActive);
