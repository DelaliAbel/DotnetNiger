using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace DotnetNiger.Gateway.Configuration;

/// <summary>Configuration d'un service aval (downstream) pour Ocelot.</summary>
public sealed class DownstreamServiceConfig
{
    public string Id { get; init; } = string.Empty;
    public string ContainerName { get; init; } = string.Empty;
    public int Port { get; init; }
    public string DevUrl { get; init; } = string.Empty;
    public string HealthEndpoint { get; init; } = string.Empty;
    public string SwaggerEndpoint { get; init; } = string.Empty;
    public string SwaggerName { get; init; } = string.Empty;
    public string RoutesConfig { get; init; } = string.Empty;
}

/// <summary>Construit la configuration Ocelot fusionnée à partir des fichiers de routes individuels.</summary>
public static class OcelotConfigurationBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>Fusionne les fichiers de routes en un seul fichier ocelot.json avec réécriture des hôtes.</summary>
    public static string BuildMergedConfig(
        string contentRootPath,
        bool useContainerHosts,
        IReadOnlyCollection<DownstreamServiceConfig> services,
        IConfiguration configuration)
    {
        var globalPath = Path.Combine(contentRootPath, "ocelot.global.json");
        if (!File.Exists(globalPath))
            throw new FileNotFoundException(Messages.Ocelot.MissingGlobalConfig);

        var globalNode = JsonNode.Parse(File.ReadAllText(globalPath))?.AsObject()
            ?? throw new InvalidOperationException(Messages.Ocelot.InvalidGlobalJson);

        var mergedRoutes = new JsonArray();

        foreach (var service in services)
        {
            var routesPath = Path.Combine(contentRootPath, service.RoutesConfig);
            if (!File.Exists(routesPath))
            {
                continue;
            }

            var serviceNode = JsonNode.Parse(File.ReadAllText(routesPath))?.AsObject();
            if (serviceNode?["Routes"] is not JsonArray routes)
                continue;

            foreach (var route in routes)
            {
                if (route != null)
                    mergedRoutes.Add(route.DeepClone());
            }
        }

        var merged = new JsonObject
        {
            ["Routes"] = mergedRoutes,
            ["GlobalConfiguration"] = globalNode["GlobalConfiguration"]?.DeepClone(),
            ["SwaggerEndPoints"] = BuildSwaggerEndPoints(services, useContainerHosts)
        };

        RewriteDownstreamHosts(mergedRoutes, services, useContainerHosts);

        var baseUrl = configuration["Gateway:BaseUrl"] ?? "http://localhost:5000";
        if (merged["GlobalConfiguration"] is JsonObject gc)
        {
            gc["BaseUrl"] = useContainerHosts ? "http://gateway:5000" : baseUrl;
        }

        var mergedPath = Path.Combine(contentRootPath, "ocelot.json");
        File.WriteAllText(mergedPath, merged.ToJsonString(JsonOptions));

        return "ocelot.json";
    }

    private static JsonArray BuildSwaggerEndPoints(IReadOnlyCollection<DownstreamServiceConfig> services, bool useContainerHosts)
    {
        var endpoints = new JsonArray();
        foreach (var service in services)
        {
            var swaggerUrl = useContainerHosts
                ? $"http://{service.ContainerName}:{service.Port}{service.SwaggerEndpoint}"
                : $"{service.DevUrl.TrimEnd('/')}{service.SwaggerEndpoint}";

            endpoints.Add(new JsonObject
            {
                ["Key"] = service.Id,
                ["Config"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["Name"] = service.SwaggerName,
                        ["Version"] = "v1",
                        ["Url"] = swaggerUrl
                    }
                }
            });
        }
        return endpoints;
    }

    private static void RewriteDownstreamHosts(JsonArray routes, IReadOnlyCollection<DownstreamServiceConfig> services, bool useContainerHosts)
    {
        foreach (var routeNode in routes.OfType<JsonObject>())
        {
            var swaggerKey = routeNode["SwaggerKey"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(swaggerKey))
                continue;

            var service = services.FirstOrDefault(s =>
                string.Equals(s.Id, swaggerKey, StringComparison.OrdinalIgnoreCase));
            if (service == null)
                continue;

            if (!Uri.TryCreate(service.DevUrl, UriKind.Absolute, out var uri))
                continue;

            var scheme = useContainerHosts ? "http" : uri.Scheme;
            var host = useContainerHosts ? service.ContainerName : uri.Host;
            var port = useContainerHosts ? service.Port : uri.Port;

            routeNode["DownstreamScheme"] = scheme;
            routeNode["DownstreamHostAndPorts"] = new JsonArray
            {
                new JsonObject
                {
                    ["Host"] = host,
                    ["Port"] = port
                }
            };
        }
    }

    /// <summary>Fusionne les fichiers de routes pour une utilisation avec Consul (découverte de services).</summary>
    public static string BuildMergedConfigWithConsul(
        string contentRootPath,
        IReadOnlyCollection<DownstreamServiceConfig> services)
    {
        var globalPath = Path.Combine(contentRootPath, "ocelot.global.json");
        if (!File.Exists(globalPath))
            throw new FileNotFoundException(Messages.Ocelot.MissingGlobalConfig);

        var globalNode = JsonNode.Parse(File.ReadAllText(globalPath))?.AsObject()
            ?? throw new InvalidOperationException(Messages.Ocelot.InvalidGlobalJson);

        var mergedRoutes = new JsonArray();

        foreach (var service in services)
        {
            var routesPath = Path.Combine(contentRootPath, service.RoutesConfig);
            if (!File.Exists(routesPath))
                continue;

            var serviceNode = JsonNode.Parse(File.ReadAllText(routesPath))?.AsObject();
            if (serviceNode?["Routes"] is not JsonArray routes)
                continue;

            foreach (var routeNode in routes.OfType<JsonObject>().Select(r => r.DeepClone().AsObject()))
            {
                routeNode.Remove("DownstreamHostAndPorts");
                routeNode["ServiceName"] = service.ContainerName;
                routeNode["UseServiceDiscovery"] = true;

                mergedRoutes.Add(routeNode);
            }
        }

        var merged = new JsonObject
        {
            ["Routes"] = mergedRoutes,
            ["GlobalConfiguration"] = globalNode["GlobalConfiguration"]?.DeepClone(),
            ["SwaggerEndPoints"] = BuildSwaggerEndPoints(services, useContainerHosts: true)
        };

        if (merged["GlobalConfiguration"] is JsonObject gc)
        {
            gc["BaseUrl"] = "http://gateway:5000";
        }

        var mergedPath = Path.Combine(contentRootPath, "ocelot.json");
        File.WriteAllText(mergedPath, merged.ToJsonString(JsonOptions));

        return "ocelot.json";
    }
}
