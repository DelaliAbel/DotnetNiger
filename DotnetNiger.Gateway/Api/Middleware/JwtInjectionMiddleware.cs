namespace DotnetNiger.Gateway.Api.Middleware;

/// <summary>
/// Middleware d'injection de JWT dans les headers
/// </summary>
public class JwtInjectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtInjectionMiddleware> _logger;

    public JwtInjectionMiddleware(RequestDelegate next, ILogger<JwtInjectionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Injecter le token JWT si disponible
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                context.Request.Headers["X-User-Id"] = userId;
                _logger.LogDebug("User ID injecté: {UserId}", userId);
            }
        }

        await _next(context);
    }
}