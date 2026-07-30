namespace DotnetNiger.Api.Middleware;

public class JsonErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public JsonErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (!context.Response.HasStarted && context.Response.StatusCode is 404 or 401 or 403 or 429 or 500
            && context.Response.ContentType == null)
        {
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
            await context.Response.WriteAsJsonAsync(new { error = message, statusCode = context.Response.StatusCode });
        }
    }
}

public static class JsonErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseJsonErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<JsonErrorHandlingMiddleware>();
    }
}
