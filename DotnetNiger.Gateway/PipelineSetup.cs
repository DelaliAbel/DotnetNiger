using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

namespace DotnetNiger.Gateway;

/// <summary>Configure le pipeline HTTP de la Gateway.</summary>
public static class PipelineSetup
{
    /// <summary>Configure CORS et les en-têtes forwarded, retourne l'application prête à démarrer.</summary>
    public static WebApplication ConfigureApp(this WebApplicationBuilder builder)
    {
        var app = builder.Build();

        app.UseCors();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedFor
        });

        return app;
    }

    /// <summary>Middleware health + swagger exécuté avant Ocelot.</summary>
    public static void UseHealthAndSwagger(this WebApplication app)
    {
        var identityUrl = app.Configuration["DownstreamServices:Identity:DevUrl"] ?? "http://localhost:5075";
        var communityUrl = app.Configuration["DownstreamServices:Community:DevUrl"] ?? "http://localhost:5050";

        app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Path == "/api/health")
            {
                var factory = ctx.RequestServices.GetRequiredService<IHttpClientFactory>();
                var identityHealth = "/health";
                var communityHealth = "/api/v1/test/health";

                var results = new Dictionary<string, object>();
                var allHealthy = true;

                using var client = factory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                foreach (var (name, baseUrl, path) in new[] {
                    ("Identity", identityUrl, identityHealth),
                    ("Community", communityUrl, communityHealth) })
                {
                    try
                    {
                        var response = await client.GetAsync($"{baseUrl.TrimEnd('/')}{path}");
                        var body = await response.Content.ReadAsStringAsync();
                        results[name] = new { status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy", code = (int)response.StatusCode, body };
                        if (!response.IsSuccessStatusCode) allHealthy = false;
                    }
                    catch (Exception ex)
                    {
                        results[name] = new { status = "Unreachable", error = ex.Message };
                        allHealthy = false;
                    }
                }

                ctx.Response.ContentType = "application/json; charset=utf-8";
                await ctx.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = allHealthy ? "Healthy" : "Degraded",
                    service = "DotnetNiger.Gateway",
                    timestamp = DateTime.UtcNow,
                    upstream = results
                }));
                return;
            }

            if (app.Environment.IsDevelopment() && ctx.Request.Path == "/swagger")
            {
                ctx.Response.ContentType = "text/html; charset=utf-8";
                await ctx.Response.WriteAsync(SwaggerAggregatedPage());
                return;
            }

            await next();
        });
    }

    static string SwaggerAggregatedPage()
    {
        return """
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8"/>
<title>DotnetNiger API — Swagger</title>
<link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css" />
</head>
<body>
<div id="swagger-ui"></div>
<script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
<script>
const ui = SwaggerUIBundle({
  urls: [
    { name: 'Identity API', url: '/openapi/identity/v1.json' },
    { name: 'Community API', url: '/openapi/community/v1.json' }
  ],
  dom_id: '#swagger-ui',
  presets: [SwaggerUIBundle.presets.apis],
  layout: 'BaseLayout'
});
</script>
</body>
</html>
""";
    }
}
