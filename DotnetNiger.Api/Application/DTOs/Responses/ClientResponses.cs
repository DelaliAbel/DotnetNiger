namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'un client OAuth.</summary>
public record OAuthClientResponse(
    // <summary>Identifiant du client.</summary>
    Guid Id,
    // <summary>Identifiant public du client.</summary>
    string ClientId,
    // <summary>Nom du client.</summary>
    string ClientName,
    // <summary>Description du client.</summary>
    string? Description,
    // <summary>URLs de redirection autorisées.</summary>
    List<string> RedirectUris,
    // <summary>URLs de redirection post-déconnexion.</summary>
    List<string> PostLogoutRedirectUris,
    // <summary>Types de grant autorisés.</summary>
    List<string> AllowedGrantTypes,
    // <summary>Indique si le client est actif.</summary>
    bool IsActive,
    // <summary>Date de création.</summary>
    DateTime CreatedAt);

/// <summary>Réponse de création d'un client OAuth avec secret.</summary>
public record OAuthClientCreatedResponse(
    // <summary>Informations du client créé.</summary>
    OAuthClientResponse Client,
    // <summary>Secret du client (affiché uniquement à la création).</summary>
    string? ClientSecret);
