using DotnetNiger.Gateway.Configuration;
using DotnetNiger.Gateway.Extensions;
using MMLib.SwaggerForOcelot.Middleware;
using Ocelot.Middleware;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Ocelot", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Gateway")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/gateway-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

Log.Information("Démarrage du DotnetNiger API Gateway...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    var services = LoadDownstreamServices(builder.Configuration);
    var mergedOcelotFile = OcelotConfigurationBuilder.BuildMergedConfig(
        builder.Environment.ContentRootPath,
        builder.Environment.IsProduction(),
        services);

    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile(mergedOcelotFile, optional: false, reloadOnChange: false);

    builder.Services.AddGatewayServices(builder.Configuration, builder.Environment);

    var app = builder.Build();

    app.UseCors("AllowAll");

    app.UseLatencyMetricsMiddleware();
    app.UseClientIdResolutionMiddleware();
    app.UseRequestTracingMiddleware();
    app.UseCustomSwaggerMergeMiddleware();

    app.MapGatewayHealthEndpoints(services);

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSwaggerForOcelotUI(opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/docs";
    }, uiOpt =>
    {
        uiOpt.EnableFilter();
        uiOpt.EnableDeepLinking();
        uiOpt.DisplayRequestDuration();
        uiOpt.EnablePersistAuthorization();
        uiOpt.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });

    await app.UseOcelot();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
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
