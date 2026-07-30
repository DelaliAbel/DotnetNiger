using System.Security.Claims;

namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Résultat d'échange d'un token OAuth.</summary>
public class TokenExchangeResult
{
    /// <summary>Identité de l'utilisateur authentifié.</summary>
    public ClaimsPrincipal? Principal { get; init; }
    /// <summary>Message d'erreur en cas d'échec.</summary>
    public string? Error { get; init; }

    /// <summary>Crée un résultat de succès avec l'identité utilisateur.</summary>
    public static TokenExchangeResult Success(ClaimsPrincipal principal) => new() { Principal = principal };
    /// <summary>Crée un résultat d'échec avec un message d'erreur.</summary>
    public static TokenExchangeResult Failure(string error) => new() { Error = error };
}
