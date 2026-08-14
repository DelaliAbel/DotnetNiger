using Microsoft.AspNetCore.Builder;

namespace DotnetNiger.Api.Middleware;

/// <summary>
/// Remplit d'un corps JSON les réponses HTTP vides produites par le pipeline
/// (401/403 des challenges et policies, 404 par défaut, 429, 500 sans corps).
/// N'intervient que si aucun corps n'a encore été écrit.
/// </summary>
public class EmptyErrorResponseMiddleware
{
    private readonly RequestDelegate _next;

    public EmptyErrorResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.HasStarted)
            return;

        if (context.Response.StatusCode is not (401 or 403 or 404 or 429 or 500))
            return;

        if (context.Response.ContentType != null)
            return;

        context.Response.ContentType = "application/json";
        var message = context.Response.StatusCode switch
        {
            401 => "Non authentifié",
            403 => "Accès refusé",
            404 => "Ressource introuvable",
            429 => "Trop de requêtes. Veuillez réessayer plus tard.",
            500 => "Erreur interne du serveur",
            _ => "Erreur"
        };
        await context.Response.WriteAsJsonAsync(new
        {
            error = message,
            statusCode = context.Response.StatusCode,
            detail = (string?)null
        });
    }
}

public static class EmptyErrorResponseMiddlewareExtensions
{
    public static IApplicationBuilder UseEmptyErrorResponses(this IApplicationBuilder app)
    {
        return app.UseMiddleware<EmptyErrorResponseMiddleware>();
    }
}
