
namespace DotnetNiger.Gateway.Api.Middleware;

/// <summary>
/// Middleware d'authentification JWT
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            _logger.LogDebug("Token JWT détecté dans la requête");
            // Validation du token se fera ici plus tard
        }

        await _next(context);
    }
}