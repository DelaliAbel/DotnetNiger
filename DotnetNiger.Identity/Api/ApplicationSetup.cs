using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using DotnetNiger.Identity.Api.Middleware;
using DotnetNiger.Identity.Infrastructure;

namespace DotnetNiger.Identity.Api;

public static class ApplicationSetup
{
    public static WebApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(ApplicationSetup).Assembly);
        builder.Services.AddRazorPages();
        builder.Services.AddHttpClient();
        builder.Services.AddProblemDetails();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddOpenApi();
        builder.Services.AddIdentityInfrastructure(builder.Configuration, builder.Environment);
        builder.Services.AddIdentityServices();
        builder.Services.AddRateLimitingPolicies(builder.Configuration);
        builder.Services.AddTransient<IClaimsTransformation, RoleClaimsTransformer>();
        builder.Services.AddApiVersioningWithSwagger();

        return builder;
    }

    public static WebApplication ConfigureApp(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
            ForwardLimit = null,
            KnownProxies = { },
            KnownNetworks = { }
        });

        app.Use((context, next) =>
        {
            if (!app.Environment.IsDevelopment())
                context.Request.Scheme = "https";
            return next();
        });

        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseRouting();
        app.UseCors("AllowFrontendOrigins");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<TenantResolutionMiddleware>();

        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'";
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            await next();
        });

        app.MapOpenApi();

        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/swagger", async (HttpContext ctx) =>
            {
                ctx.Response.ContentType = "text/html; charset=utf-8";
                await ctx.Response.WriteAsync(SwaggerUiPage("/openapi/v1.json", "DotnetNiger Identity v1"));
            });
        }

        app.MapGet("/health", () => Results.Ok(new
        {
            status = "Healthy",
            service = "DotnetNiger.Identity",
            timestamp = DateTime.UtcNow
        }));

        app.MapGet("/health/ready", async ([FromServices] IdentityDbContext idCtx) =>
        {
            try
            {
                await idCtx.Database.CanConnectAsync();

                return Results.Ok(new
                {
                    status = "Ready",
                    service = "DotnetNiger.Identity",
                    timestamp = DateTime.UtcNow,
                    checks = new
                    {
                        database = "connected"
                    }
                });
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Health check failed: database unreachable");
                return Results.StatusCode(503);
            }
        });

        app.MapGet("/health/downstream", async ([FromServices] IdentityDbContext idCtx,
            [FromServices] IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                await idCtx.Database.CanConnectAsync();

                return Results.Ok(new
                {
                    status = "Healthy",
                    service = "DotnetNiger.Identity",
                    timestamp = DateTime.UtcNow,
                    checks = new
                    {
                        database = "connected",
                        downstream = "not_checked"
                    }
                });
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Downstream health check failed: database unreachable");
                return Results.StatusCode(503);
            }
        });

        app.MapControllers();
        app.MapRazorPages();

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
