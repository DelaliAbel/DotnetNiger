using DotnetNiger.Gateway.Application.Services;
using DotnetNiger.Gateway.Application.Services.Interfaces;

namespace DotnetNiger.Gateway.Api.Middleware;

/// <summary>
/// Middleware d'authentification JWT - valide les tokens et enrichit le contexte
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    // Routes publiques qui ne nécessitent pas d'authentification
    private static readonly string[] PublicPaths = new[]
    {
        "/swagger",
        "/health",
        "/identity/api/v1/auth",
        "/identity/api/v1/diagnostics",
        "/identity/api/v1/tokens/refresh",
        "/community/api/v1/diagnostics"
    };

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuthenticationService authService)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Routes publiques - pas de validation requise
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            // Pas de token - laisse le service cible décider (certains endpoints peuvent être publics)
            await _next(context);
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Valider le token
        var isValid = await authService.ValidateTokenAsync(token);
        if (!isValid)
        {
            _logger.LogWarning("Token JWT invalide pour {Path}", path);
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Token invalide ou expiré\"}");
            return;
        }

        // Enrichir le contexte avec les claims
        var principal = authService.GetClaimsPrincipal(token);
        if (principal != null)
        {
            context.User = principal;

            // Ajouter les headers pour les services downstream
            var userId = await authService.GetUserIdFromTokenAsync(token);
            if (!string.IsNullOrEmpty(userId))
            {
                context.Request.Headers["X-User-Id"] = userId;
            }
        }

        await _next(context);
    }

    private static bool IsPublicPath(string path)
    {
        return PublicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}
