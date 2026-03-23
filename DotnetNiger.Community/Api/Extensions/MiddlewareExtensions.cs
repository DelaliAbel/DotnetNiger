using DotnetNiger.Community.Api.Middleware;
using Microsoft.AspNetCore.Builder;

namespace DotnetNiger.Community.Api.Extensions;

/// <summary>
/// Extensions pour enregistrer les middlewares Community
/// </summary>
public static class MiddlewareExtensions
{
	/// <summary>
	/// Ajoute le middleware de validation JWT
	/// </summary>
	public static IApplicationBuilder UseJwtValidation(this IApplicationBuilder app)
	{
		return app.UseMiddleware<JwtValidationMiddleware>();
	}

	/// <summary>
	/// Ajoute le middleware d'enregistrement des requêtes
	/// </summary>
	public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
	{
		return app.UseMiddleware<RequestLoggingMiddleware>();
	}

	/// <summary>
	/// Ajoute le middleware de gestion des erreurs
	/// </summary>
	public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
	{
		return app.UseMiddleware<ErrorHandlingMiddleware>();
	}

	/// <summary>
	/// Configure tous les middlewares personnalisés pour l'API Community
	/// </summary>
	public static IApplicationBuilder UseCommunityMiddleware(this IApplicationBuilder app)
	{
		app.UseErrorHandling();
		app.UseRequestLogging();
		app.UseJwtValidation();

		return app;
	}
}
