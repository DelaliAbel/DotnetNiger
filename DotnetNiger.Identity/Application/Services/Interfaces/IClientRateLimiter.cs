namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IClientRateLimiter
{
    bool TryConsume(string action, string clientId, int maxAttempts, TimeSpan window, out TimeSpan? retryAfter);
}
