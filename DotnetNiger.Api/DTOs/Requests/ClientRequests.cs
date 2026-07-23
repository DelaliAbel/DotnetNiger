using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de création d'un client OAuth.</summary>
public record CreateOAuthClientRequest(
    // <summary>Nom du client (1-100 caractères).</summary>
    [Required][StringLength(100, MinimumLength = 1)] string ClientName,
    // <summary>Description du client.</summary>
    string? Description,
    // <summary>URLs de redirection autorisées.</summary>
    string? RedirectUris,
    // <summary>URLs de redirection post-déconnexion.</summary>
    string? PostLogoutRedirectUris,
    // <summary>Types de grant autorisés.</summary>
    string? AllowedGrantTypes);

/// <summary>Requête de mise à jour d'un client OAuth.</summary>
public record UpdateOAuthClientRequest(
    // <summary>Nom du client.</summary>
    string? ClientName,
    // <summary>Description du client.</summary>
    string? Description,
    // <summary>URLs de redirection autorisées.</summary>
    string? RedirectUris,
    // <summary>URLs de redirection post-déconnexion.</summary>
    string? PostLogoutRedirectUris,
    // <summary>Types de grant autorisés.</summary>
    string? AllowedGrantTypes,
    // <summary>Indique si le client est actif.</summary>
    bool? IsActive);
