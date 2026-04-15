// Middleware API Identity: JwtMiddleware
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Identity.Api.Middleware;

/// <summary>
/// Middleware JWT custom qui enrichit le contexte HTTP avec les informations
/// extraites du token JWT authentifie :
/// - Ajoute les headers X-User-Id, X-User-Email, X-User-Roles pour le logging/audit.
/// - Injecte le UserId dans HttpContext.Items pour un acces simplifie.
/// - Log les acces authentifies et les tentatives avec token expire/invalide.
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = context.User.FindFirstValue(ClaimTypes.Email);
            var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);

            // Injecter dans HttpContext.Items pour acces rapide dans les services.
            if (!string.IsNullOrEmpty(userId))
            {
                context.Items["UserId"] = userId;
                context.Response.Headers["X-User-Id"] = userId;
            }

            if (!string.IsNullOrEmpty(email))
            {
                context.Items["UserEmail"] = email;
                context.Response.Headers["X-User-Email"] = email;
            }

            var roleList = string.Join(",", roles);
            if (!string.IsNullOrEmpty(roleList))
            {
                context.Items["UserRoles"] = roleList;
                context.Response.Headers["X-User-Roles"] = roleList;
            }

            _logger.LogDebug(
                "JWT authentifie — UserId={UserId}, Email={Email}, Roles={Roles}",
                userId, email, roleList);
        }
        else if (context.Request.Headers.ContainsKey("Authorization"))
        {
            // Un header Authorization est present mais l'utilisateur n'est pas authentifie
            // => token expire, invalide ou malformed.
            _logger.LogWarning(
                "Token JWT present mais non authentifie pour {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }

        await _next(context);
    }
}
