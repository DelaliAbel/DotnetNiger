using System.Text.Json;
using DotnetNiger.Gateway.Application.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetNiger.Gateway.Application.Services;

/// <summary>
/// Gère le cache des réponses
/// </summary>
public class CachingService : ICachingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingService> _logger;

    public CachingService(IMemoryCache cache, ILogger<CachingService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return Task.FromResult(value);
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };

        _cache.Set(key, value, options);
        _logger.LogDebug("Cached value for key: {Key}", key);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        _logger.LogDebug("Removed cache for key: {Key}", key);
        return Task.CompletedTask;
    }
}
