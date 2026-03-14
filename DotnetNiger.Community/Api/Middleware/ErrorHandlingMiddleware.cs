using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Middleware;

public class ErrorHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ErrorHandlingMiddleware> _logger;

	public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception exception)
		{
			await HandleExceptionAsync(context, exception);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		var (statusCode, title) = exception switch
		{
			UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Non autorise"),
			KeyNotFoundException => (StatusCodes.Status404NotFound, "Ressource introuvable"),
			ValidationException => (StatusCodes.Status400BadRequest, "Validation echouee"),
			ArgumentException => (StatusCodes.Status400BadRequest, "Requete invalide"),
			_ => (StatusCodes.Status500InternalServerError, "Erreur interne du serveur")
		};

		if (statusCode >= StatusCodes.Status500InternalServerError)
		{
			_logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);
		}

		var problem = new ProblemDetails
		{
			Title = title,
			Detail = exception.Message,
			Status = statusCode,
			Instance = context.Request.Path
		};
		problem.Extensions["traceId"] = context.TraceIdentifier;

		context.Response.StatusCode = statusCode;
		context.Response.ContentType = "application/problem+json";
		await context.Response.WriteAsJsonAsync(problem);
	}
}
