/// <summary>Point d'entrée du API Gateway DotnetNiger.</summary>
using DotnetNiger.Gateway.Configuration;
using DotnetNiger.Gateway.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using DotnetNiger.Gateway.Services;
using Ocelot.Middleware;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();

    var seedServices = LoadDownstreamServices(builder.Configuration);
    var useConsul = string.Equals(
        builder.Configuration["ServiceDiscovery:Provider"], "Consul", StringComparison.OrdinalIgnoreCase);
    var useContainerHosts = string.Equals(
        builder.Configuration["ServiceDiscovery:UseContainerHosts"], "true", StringComparison.OrdinalIgnoreCase);

    var mergedOcelotFile = useConsul
        ? OcelotConfigurationBuilder.BuildMergedConfigWithConsul(
            builder.Environment.ContentRootPath, seedServices)
        : OcelotConfigurationBuilder.BuildMergedConfig(
            builder.Environment.ContentRootPath,
            useContainerHosts,
            seedServices,
            builder.Configuration);

    var serviceRegistry = new ServiceRegistry(seedServices);
    builder.Services.AddSingleton<IServiceRegistry>(serviceRegistry);

    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile(mergedOcelotFile, optional: false, reloadOnChange: false);

    builder.Services.AddGatewayServices(builder.Configuration, builder.Environment);

    var app = builder.Build();

    app.Use(async (ctx, next) =>
    {
        if (ctx.Request.Path == "/swagger" && ctx.Request.Method == "GET")
        {
            ctx.Response.ContentType = "text/html; charset=utf-8";
            await ctx.Response.WriteAsync(SwaggerUiPage("/swagger/docs/v1/all", "DotnetNiger Gateway API"));
            return;
        }
        await next();
    });

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
    });
    app.UseCors("AllowAll");
    app.UseSecurityHeadersMiddleware();
    app.UseTokenCookieMiddleware();

    app.UseLatencyMetricsMiddleware();
    app.UseClientIdResolutionMiddleware();
    app.UseRequestTracingMiddleware();
    app.UseOpenGraphMiddleware();
    app.UseCustomSwaggerMergeMiddleware();
    app.UseExternalServiceProxy();

    app.MapGatewayHealthEndpoints();
    app.MapServiceRegistryEndpoint();
    app.MapCacheBusterEndpoint();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                await context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        error = Messages.Common.InternalServerError,
                        statusCode = 500
                    }));
            });
        });
    }

    app.Use(async (context, next) =>
    {
        context.Request.Headers["X-Forwarded-Proto"] = context.Request.Scheme;
        context.Request.Headers["X-Forwarded-Host"] = context.Request.Host.Host;
        await next();
    });

    app.UseAuthentication();
    app.UseAuthorization();

    await app.UseOcelot();
    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger("Program");
    logger.LogCritical(ex, "Application terminated unexpectedly");
    return 1;
}

static List<DownstreamServiceConfig> LoadDownstreamServices(IConfiguration configuration)
{
    var services = new List<DownstreamServiceConfig>();
    var section = configuration.GetSection("DownstreamServices").GetChildren();

    foreach (var child in section)
    {
        var id = child["Id"] ?? child.Key.ToLowerInvariant();
        var containerName = child["ContainerName"] ?? id;
        var port = int.TryParse(child["Port"], out var p) ? p : 8080;
        var devUrl = child["DevUrl"] ?? $"http://localhost:{port}";
        var healthEndpoint = child["HealthEndpoint"] ?? "/health";
        var swaggerEndpoint = child["SwaggerEndpoint"] ?? "/swagger/v1/swagger.json";
        var swaggerName = child["SwaggerName"] ?? $"{id} API";
        var routesConfig = child["RoutesConfig"] ?? $"ocelot.{id}.routes.json";

        services.Add(new DownstreamServiceConfig
        {
            Id = id,
            ContainerName = containerName,
            Port = port,
            DevUrl = devUrl,
            HealthEndpoint = healthEndpoint,
            SwaggerEndpoint = swaggerEndpoint,
            SwaggerName = swaggerName,
            RoutesConfig = routesConfig
        });
    }

    return services;
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
