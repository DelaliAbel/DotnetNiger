namespace DotnetNiger.Gateway.Api.Middleware;

/// <summary>
/// Transforme les requêtes
/// </summary>
public class RequestTransformMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTransformMiddleware> _logger;

    public RequestTransformMiddleware(RequestDelegate next, ILogger<RequestTransformMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Ajouter des headers personnalisés
        context.Request.Headers["X-Gateway-Version"] = "1.0";
        context.Request.Headers["X-Request-Id"] = Guid.NewGuid().ToString();

        await _next(context);
    }
}
