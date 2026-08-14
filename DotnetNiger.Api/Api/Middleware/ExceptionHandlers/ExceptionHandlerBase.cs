using Microsoft.AspNetCore.Diagnostics;

namespace DotnetNiger.Api.Middleware.ExceptionHandlers;

/// <summary>
/// Base des gestionnaires d'exceptions. Le format de réponse est volontairement
/// le même que celui utilisé par le reste de l'API ({ error, statusCode, detail })
/// afin de ne pas casser le contrat consommé par le frontend.
/// </summary>
public abstract class ExceptionHandlerBase : IExceptionHandler
{
    public abstract ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken);

    protected static async ValueTask WriteErrorAsync(
        HttpContext context, int statusCode, string message,
        string? detail = null, CancellationToken cancellationToken = default)
    {
        if (context.Response.HasStarted) return;
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = message,
            statusCode,
            detail
        }, cancellationToken);
    }
}
