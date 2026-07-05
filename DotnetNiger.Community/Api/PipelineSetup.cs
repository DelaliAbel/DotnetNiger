using DotnetNiger.Community.Api.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

namespace DotnetNiger.Community.Api;

/// <summary>Configure le pipeline middleware de l'application Community.</summary>
public static class PipelineSetup
{
    /// <summary>Configure le pipeline HTTP.</summary>
    public static WebApplication ConfigureApp(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.MapOpenApi();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/swagger", async (HttpContext ctx) =>
            {
                ctx.Response.ContentType = "text/html; charset=utf-8";
                await ctx.Response.WriteAsync(SwaggerUiPage("/openapi/v1.json", "DotnetNiger Community API v1"));
            });
        }

        app.UseStaticFiles();
        app.UseCors("AllowFrontendOrigins");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    static string SwaggerUiPage(string specUrl, string title)
    {
        return $$"""
<!DOCTYPE html>
<html lang="en">
<head><meta charset="utf-8"/><title>{{title}}</title>
<link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css" />
</head>
<body>
<div id="swagger-ui"></div>
<script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
<script>
SwaggerUIBundle({ url: '{{specUrl}}', dom_id: '#swagger-ui', presets: [SwaggerUIBundle.presets.apis], layout: 'BaseLayout' });
</script>
</body>
</html>
""";
    }
}
