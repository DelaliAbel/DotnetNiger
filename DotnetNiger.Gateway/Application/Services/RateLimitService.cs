using DotnetNiger.Gateway.Application.Services.Interfaces;

namespace DotnetNiger.Gateway.Application.Services;

/// <summary>
/// Gère les limites de requêtes
/// </summary>
public class RateLimitService : IRateLimitService
{
    private readonly ILogger<RateLimitService> _logger;
    private readonly Dictionary<string, int> _requestCounts = new();

    public RateLimitService(ILogger<RateLimitService> logger)
    {
        _logger = logger;
    }

    public Task<bool> IsRequestAllowedAsync(string clientId, string endpoint)
    {
        var key = $"{clientId}:{endpoint}";

        if (_requestCounts.TryGetValue(key, out var count))
        {
            // Limite de 100 requêtes par minute (exemple)
            return Task.FromResult(count < 100);
        }

        return Task.FromResult(true);
    }

    public Task IncrementRequestCountAsync(string clientId, string endpoint)
    {
        var key = $"{clientId}:{endpoint}";

        if (_requestCounts.ContainsKey(key))
            _requestCounts[key]++;
        else
            _requestCounts[key] = 1;

        return Task.CompletedTask;
    }
}
