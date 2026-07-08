using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace DotnetNiger.Gateway;

/// <summary>Fusionne les fichiers de configuration Ocelot (global + routes) en une seule configuration.</summary>
public static class OcelotConfigMerger
{
    private static readonly string ConfigDir = AppContext.BaseDirectory;

    /// <summary>Fusionne ocelot.global.json, ocelot.identity.routes.json et ocelot.community.routes.json
    /// et substitue les hôtes downstream depuis DownstreamServices si configurés.</summary>
    public static IConfiguration Merge(IConfiguration appConfig)
    {
        var global = JsonNode.Parse(File.ReadAllText(Path.Combine(ConfigDir, "ocelot.global.json")))!.AsObject();
        var identity = JsonNode.Parse(File.ReadAllText(Path.Combine(ConfigDir, "ocelot.identity.routes.json")))!.AsObject();
        var community = JsonNode.Parse(File.ReadAllText(Path.Combine(ConfigDir, "ocelot.community.routes.json")))!.AsObject();

        var downstreamServices = appConfig.GetSection("DownstreamServices").GetChildren()
            .ToDictionary(x => x.Key, x => x["DevUrl"], StringComparer.OrdinalIgnoreCase);

        var result = new JsonObject();

        if (global.TryGetPropertyValue("GlobalConfiguration", out var gc))
            result["GlobalConfiguration"] = gc!.DeepClone();

        var allRoutes = new JsonArray();
        if (identity.TryGetPropertyValue("Routes", out var ir) && ir is JsonArray irArray)
            foreach (var r in irArray) allRoutes.Add(ResolveHost(r!.DeepClone(), downstreamServices));
        if (community.TryGetPropertyValue("Routes", out var cr) && cr is JsonArray crArray)
            foreach (var r in crArray) allRoutes.Add(ResolveHost(r!.DeepClone(), downstreamServices));
        result["Routes"] = allRoutes;

        var json = result.ToJsonString();
        return new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();
    }

    private static JsonNode? ResolveHost(JsonNode? route, Dictionary<string, string?> downstreamServices)
    {
        if (route is not JsonObject obj) return route;

        var swaggerKey = obj["SwaggerKey"]?.GetValue<string>();
        if (swaggerKey == null || !downstreamServices.TryGetValue(swaggerKey, out var devUrl) || string.IsNullOrWhiteSpace(devUrl))
            return route;

        if (!Uri.TryCreate(devUrl, UriKind.Absolute, out var uri))
            return route;

        obj["DownstreamHostAndPorts"] = new JsonArray(
            new JsonObject
            {
                ["Host"] = JsonValue.Create(uri.Host),
                ["Port"] = JsonValue.Create(uri.Port)
            }
        );

        obj["DownstreamScheme"] = JsonValue.Create(uri.Scheme);

        return route;
    }
}
