using System.Collections.Concurrent;
using DotnetNiger.Gateway.Application.Services.Interfaces;

namespace DotnetNiger.Gateway.Application.Services;

/// <summary>
/// Gère les limites de requêtes (thread-safe avec ConcurrentDictionary).
/// </summary>
public sealed class RateLimitService : IRateLimitService
{
    private readonly ILogger<RateLimitService> _logger;
    private readonly ConcurrentDictionary<string, RateLimitEntry> _requestCounts = new();
    private readonly int _maxRequestsPerMinute;

    public RateLimitService(ILogger<RateLimitService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _maxRequestsPerMinute = configuration.GetValue("RateLimit:MaxRequestsPerMinute", 100);
    }

    public Task<bool> IsRequestAllowedAsync(string clientId, string endpoint)
    {
        var key = $"{clientId}:{endpoint}";
        var now = DateTime.UtcNow;

        var entry = _requestCounts.GetOrAdd(key, _ => new RateLimitEntry());

        // Reset si la fenêtre d'une minute est passée
        if ((now - entry.WindowStart).TotalMinutes >= 1)
        {
            entry.Reset(now);
        }

        var isAllowed = entry.Count < _maxRequestsPerMinute;

        if (!isAllowed)
        {
            _logger.LogWarning("Rate limit atteint pour {Key}: {Count}/{Max}",
                key, entry.Count, _maxRequestsPerMinute);
        }

        return Task.FromResult(isAllowed);
    }

    public Task IncrementRequestCountAsync(string clientId, string endpoint)
    {
        var key = $"{clientId}:{endpoint}";

        _requestCounts.AddOrUpdate(
            key,
            _ => new RateLimitEntry { Count = 1 },
            (_, existing) =>
            {
                Interlocked.Increment(ref existing.Count);
                return existing;
            });

        return Task.CompletedTask;
    }

    private class RateLimitEntry
    {
        public int Count;
        public DateTime WindowStart = DateTime.UtcNow;

        public void Reset(DateTime newStart)
        {
            Count = 0;
            WindowStart = newStart;
        }
    }
}
