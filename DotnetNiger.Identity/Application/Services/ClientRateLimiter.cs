using DotnetNiger.Identity.Application.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetNiger.Identity.Application.Services;

public class ClientRateLimiter : IClientRateLimiter
{
    private readonly IMemoryCache _cache;

    public ClientRateLimiter(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryConsume(string action, string clientId, int maxAttempts, TimeSpan window, out TimeSpan? retryAfter)
    {
        retryAfter = null;
        var key = $"rl:{action}:{clientId}";

        if (_cache.TryGetValue<RateLimitState>(key, out var state) && state != null)
        {
            if (state.Attempts >= maxAttempts)
            {
                retryAfter = state.ExpiresAtUtc - DateTime.UtcNow;
                if (retryAfter < TimeSpan.Zero)
                {
                    retryAfter = TimeSpan.Zero;
                }

                return false;
            }

            state.Attempts++;
            _cache.Set(key, state, state.ExpiresAtUtc);
            return true;
        }

        var expiresAt = DateTime.UtcNow.Add(window);
        _cache.Set(key, new RateLimitState { Attempts = 1, ExpiresAtUtc = expiresAt }, expiresAt);
        return true;
    }

    private sealed class RateLimitState
    {
        public int Attempts { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}
