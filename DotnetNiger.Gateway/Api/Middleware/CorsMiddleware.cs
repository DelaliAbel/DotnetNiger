namespace DotnetNiger.Gateway.Api.Middleware;

/// <summary>
/// Gère les CORS
/// </summary>
public class CorsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorsMiddleware> _logger;

    public CorsMiddleware(RequestDelegate next, ILogger<CorsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // CORS est généralement géré par la configuration ASP.NET Core
        // Ce middleware peut être utilisé pour des règles CORS personnalisées
        await _next(context);
    }
}
