using System.Text.Json;
using DotnetNiger.Gateway.Configuration;
using Serilog;

namespace DotnetNiger.Gateway.Services;

/// <summary>Endpoint dynamique permettant aux services de s'enregistrer auprès du Gateway.</summary>
public static class ServiceRegistrationEndpoint
{
    /// <summary>Mappe l'endpoint /api/service-registry/register pour l'enregistrement des services.</summary>
    public static IApplicationBuilder MapServiceRegistryEndpoint(this IApplicationBuilder app)
    {
        app.Map("/api/service-registry", registryApp =>
        {
            registryApp.Run(async context =>
            {
                if (!context.Request.Path.Equals("/register", StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
                var key = configuration["Gateway:RegistrationKey"] ?? configuration["Jwt:Key"];

                if (!string.IsNullOrWhiteSpace(key) && !key.StartsWith("__"))
                {
                    var providedKey = context.Request.Headers["X-Registration-Key"].FirstOrDefault();
                    if (string.IsNullOrEmpty(providedKey) || providedKey != key)
                    {
                        Log.Warning("Service registration rejected: invalid or missing X-Registration-Key");
                        context.Response.StatusCode = 401;
                        return;
                    }
                }

                ServiceRegistrationRequest? request;
                try
                {
                    request = await JsonSerializer.DeserializeAsync<ServiceRegistrationRequest>(
                        context.Request.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync($"{{\"error\":\"{Messages.Registration.InvalidJson}\"}}");
                    return;
                }

                if (request == null || string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Url))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync($"{{\"error\":\"{Messages.Registration.IdAndUrlRequired}\"}}");
                    return;
                }

                var registry = context.RequestServices.GetRequiredService<IServiceRegistry>();

                var registration = new ServiceRegistration
                {
                    Id = request.Id.ToLowerInvariant(),
                    Url = request.Url.TrimEnd('/'),
                    Name = request.Name,
                    HealthEndpoint = request.HealthEndpoint,
                    SwaggerEndpoint = request.SwaggerEndpoint,
                    ContainerName = request.ContainerName,
                    Port = request.Port
                };

                registry.RegisterOrUpdate(registration);

                Log.Information("Service registered: {Id} @ {Url}", registration.Id, registration.Url);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { status = "registered", serviceId = registration.Id }));
            });
        });

        return app;
    }
}

/// <summary>Requête d'enregistrement d'un service auprès du Gateway.</summary>
public sealed class ServiceRegistrationRequest
{
    public string? Id { get; init; }
    public string? Url { get; init; }
    public string? Name { get; init; }
    public string? HealthEndpoint { get; init; }
    public string? SwaggerEndpoint { get; init; }
    public string? ContainerName { get; init; }
    public int? Port { get; init; }
}
