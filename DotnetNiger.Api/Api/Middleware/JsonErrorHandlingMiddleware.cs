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
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";

            var (statusCode, message) = ex switch
            {
                UnauthorizedAccessException => (403, "Accès refusé"),
                KeyNotFoundException => (404, "Ressource introuvable"),
                InvalidOperationException => (400, ex.Message),
                _ => (500, "Erreur interne du serveur")
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new
            {
                error = message,
                statusCode,
                detail = context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
                    ? ex.ToString()
                    : null
            });
            return;
        }

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
            await context.Response.WriteAsJsonAsync(new { error = message, statusCode = context.Response.StatusCode, detail = (string?)null });
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
