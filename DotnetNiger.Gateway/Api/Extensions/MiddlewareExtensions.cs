using DotnetNiger.Gateway.Api.Middleware;

namespace DotnetNiger.Gateway.Api.Extensions;

/// <summary>
/// Enregistre les middlewares
/// </summary>
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGatewayMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<RequestTransformMiddleware>();
        app.UseMiddleware<AuthenticationMiddleware>();
        app.UseMiddleware<JwtInjectionMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseMiddleware<CorsMiddleware>();

        return app;
    }
}
