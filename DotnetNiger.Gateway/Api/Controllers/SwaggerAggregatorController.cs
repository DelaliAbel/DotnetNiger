using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DotnetNiger.Gateway.Api.Controllers;

[ApiController]
[Route("swagger-aggregated")]
public class SwaggerAggregatorController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SwaggerAggregatorController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("v1/swagger.json")]
    public async Task<IActionResult> GetAggregatedSwagger()
    {
        var identityUrl = _configuration.GetSection("ReverseProxy:Clusters:identity-cluster:Destinations:destination1:Address").Value ?? "http://localhost:5075/";
        var communityUrl = _configuration.GetSection("ReverseProxy:Clusters:community-cluster:Destinations:destination1:Address").Value ?? "http://localhost:5269/";

        var services = new[]
        {
            ("identity", $"{identityUrl}swagger/v1/swagger.json"),
            ("community", $"{communityUrl}swagger/v1/swagger.json")
        };

        var aggregatedPaths = new Dictionary<string, object>();
        var aggregatedSchemas = new Dictionary<string, object>();
        var client = _httpClientFactory.CreateClient();

        foreach (var (prefix, url) in services)
        {
            try
            {
                var json = await client.GetStringAsync(url);
                var doc = JsonDocument.Parse(json);

                // Agréger les paths
                if (doc.RootElement.TryGetProperty("paths", out var paths))
                {
                    foreach (var path in paths.EnumerateObject())
                    {
                        var newPath = $"/{prefix}{path.Name}";
                        var deserialized = JsonSerializer.Deserialize<object>(path.Value.GetRawText());
                        if (deserialized != null)
                            aggregatedPaths[newPath] = deserialized;
                    }
                }

                // Agréger les schémas (components/schemas)
                if (doc.RootElement.TryGetProperty("components", out var components) &&
                    components.TryGetProperty("schemas", out var schemas))
                {
                    foreach (var schema in schemas.EnumerateObject())
                    {
                        var schemaName = schema.Name;
                        // Éviter les doublons en préfixant si nécessaire
                        if (!aggregatedSchemas.ContainsKey(schemaName))
                        {
                            var deserialized = JsonSerializer.Deserialize<object>(schema.Value.GetRawText());
                            if (deserialized != null)
                                aggregatedSchemas[schemaName] = deserialized;
                        }
                    }
                }
            }
            catch
            {
                // Service non disponible, continuer
            }
        }

        var result = new
        {
            openapi = "3.0.0",
            info = new
            {
                title = "DotnetNiger Gateway - Tous les services",
                version = "v1"
            },
            paths = aggregatedPaths,
            components = new
            {
                schemas = aggregatedSchemas
            }
        };

        return Ok(result);
    }
}
