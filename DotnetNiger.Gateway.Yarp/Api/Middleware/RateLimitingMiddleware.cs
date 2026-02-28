namespace DotnetNiger.Gateway.Api.Middleware;

/// <summary>
/// Middleware de limitation de débit (rate limiting)
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Rate limiting sera implémenté plus tard avec Redis
        // Pour l'instant, on laisse passer toutes les requêtes
        
        await _next(context);
    }
}