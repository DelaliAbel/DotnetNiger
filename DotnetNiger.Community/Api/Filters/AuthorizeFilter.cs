using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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
		var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
		var expectedApiKey = configuration["Admin:ApiKey"];

		if (string.IsNullOrWhiteSpace(expectedApiKey))
		{
			context.Result = new ObjectResult(new
			{
				message = "Configuration admin manquante",
				details = "La cle Admin:ApiKey doit etre definie via secret ou variable d'environnement"
			})
			{
				StatusCode = StatusCodes.Status500InternalServerError
			};
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
}
