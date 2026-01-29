namespace DotnetNiger.Gateway.Application.Services;

/// <summary>
/// Interface pour le service d'authentification
/// </summary>
public interface IAuthenticationService
{
    Task<bool> ValidateTokenAsync(string token);
    Task<string?> GetUserIdFromTokenAsync(string token);
}
