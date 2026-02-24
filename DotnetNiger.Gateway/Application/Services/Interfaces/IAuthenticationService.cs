using System.Security.Claims;

namespace DotnetNiger.Gateway.Application.Services.Interfaces;

/// <summary>
/// Interface pour le service d'authentification JWT
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Valide un token JWT
    /// </summary>
    Task<bool> ValidateTokenAsync(string token);

    /// <summary>
    /// Extrait l'ID utilisateur du token
    /// </summary>
    Task<string?> GetUserIdFromTokenAsync(string token);

    /// <summary>
    /// Extrait les claims du token pour enrichir le contexte
    /// </summary>
    ClaimsPrincipal? GetClaimsPrincipal(string token);
}
