using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace DotnetNiger.Community.Api.Filters;

/// <summary>
/// Filtre d'autorisation simple pour les endpoints admin.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class AuthorizeFilter : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[] _allowedRoles;

    public AuthorizeFilter(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles ?? Array.Empty<string>();
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (UserHasAllowedRole(context.HttpContext.User))
        {
            return Task.CompletedTask;
        }

        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expectedApiKey = configuration["Admin:ApiKey"];

        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Acces admin refuse",
                details = "Aucun role admin valide dans le JWT et configuration Admin:ApiKey absente"
            });
            return Task.CompletedTask;
        }

        var providedApiKey = context.HttpContext.Request.Headers["X-Admin-Key"].ToString();
        if (!string.Equals(providedApiKey, expectedApiKey, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Acces admin refuse",
                details = "X-Admin-Key invalide ou manquant"
            });
            return Task.CompletedTask;
        }

        if (_allowedRoles.Length > 0)
        {
            var providedRole = context.HttpContext.Request.Headers["X-Admin-Role"].ToString();
            var roleIsAllowed = _allowedRoles.Any(role =>
                string.Equals(role, providedRole, StringComparison.OrdinalIgnoreCase));

            if (!roleIsAllowed)
            {
                context.Result = new ForbidResult();
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }

    private bool UserHasAllowedRole(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (_allowedRoles.Length == 0)
        {
            return true;
        }

        var roleClaimTypes = new[]
        {
            ClaimTypes.Role,
            "role",
            "roles",
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };

        var userRoles = user.Claims
            .Where(c => roleClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return _allowedRoles.Any(role => userRoles.Contains(role));
    }
}
