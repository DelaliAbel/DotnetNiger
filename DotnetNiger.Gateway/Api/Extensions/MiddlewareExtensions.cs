using DotnetNiger.Gateway.Api.Middleware;

namespace DotnetNiger.Gateway.Api.Extensions;

/// <summary>
/// Enregistre les middlewares
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Configure le pipeline de middlewares du Gateway.
    /// Ordre: ErrorHandling → Logging → Transform → Auth → RateLimiting
    /// </summary>
    public static IApplicationBuilder UseGatewayMiddlewares(this IApplicationBuilder app)
    {
        // 1. Gestion des erreurs (doit être premier pour capturer toutes les exceptions)
        app.UseMiddleware<ErrorHandlingMiddleware>();

        // 2. Logging des requêtes/réponses
        app.UseMiddleware<RequestLoggingMiddleware>();

        // 3. Transformation des requêtes (headers X-Gateway-Version, X-Request-Id)
        app.UseMiddleware<RequestTransformMiddleware>();

        // 4. Authentification JWT (valide tokens et injecte X-User-Id)
        app.UseMiddleware<AuthenticationMiddleware>();

        // 5. Rate limiting
        app.UseMiddleware<RateLimitingMiddleware>();

        // Note: CORS est géré par app.UseCors() dans Program.cs

        return app;
    }
}
