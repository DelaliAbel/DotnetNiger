namespace DotnetNiger.Gateway.Application.Services.Interfaces;

/// <summary>
/// Interface pour le service de limitation de taux
/// </summary>
public interface IRateLimitService
{
    Task<bool> IsRequestAllowedAsync(string clientId, string endpoint);
    Task IncrementRequestCountAsync(string clientId, string endpoint);
}
