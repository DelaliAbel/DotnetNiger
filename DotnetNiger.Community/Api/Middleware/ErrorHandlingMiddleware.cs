using System.Net;
using System.Text.Json;

namespace DotnetNiger.Community.Api.Middleware;

/// <summary>Intercepte les exceptions non gérées et retourne une réponse JSON structurée.</summary>
public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    /// <summary>Exécute le middleware : capture les exceptions et les transforme en réponses HTTP appropriées.</summary>
    /// <param name="context">Contexte HTTP de la requête.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            if (!context.Response.HasStarted)
                await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>Construit et écrit la réponse d'erreur formatée (problem+json).</summary>
    /// <param name="context">Contexte HTTP.</param>
    /// <param name="exception">Exception capturée.</param>
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, detail) = exception switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, exception.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Une erreur interne est survenue.")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://httpstatuses.io/{(int)statusCode}",
            title = statusCode switch
            {
                HttpStatusCode.NotFound => "Not Found",
                HttpStatusCode.BadRequest => "Bad Request",
                HttpStatusCode.Forbidden => "Forbidden",
                _ => "Internal Server Error"
            },
            status = (int)statusCode,
            detail,
            instance = context.Request.Path
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
