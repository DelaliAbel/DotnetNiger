using System.Security.Claims;

namespace DotnetNiger.Community.Api.Middleware;

/// <summary>
/// Middleware pour valider et enrichir les tokens JWT
/// Extrait les claims du token et les ajoute au contexte utilisateur
/// </summary>
public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtValidationMiddleware> _logger;

    public JwtValidationMiddleware(RequestDelegate next, ILogger<JwtValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrWhiteSpace(userId))
                    context.Items["UserId"] = userId;

                var userEmail = context.User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrWhiteSpace(userEmail))
                    context.Response.Headers["X-User-Email"] = userEmail;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT in middleware");
            await _next(context);
        }
    }
}
