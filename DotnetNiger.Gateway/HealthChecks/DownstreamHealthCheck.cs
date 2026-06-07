using Microsoft.Extensions.Diagnostics.HealthChecks;
using DotnetNiger.Gateway.Configuration;
using DotnetNiger.Gateway.Services;

namespace DotnetNiger.Gateway.HealthChecks;

public class DownstreamHealthCheck : IHealthCheck
{
    private readonly IServiceRegistry _registry;
    private readonly IHttpClientFactory _factory;

    public DownstreamHealthCheck(IServiceRegistry registry, IHttpClientFactory factory)
    {
        _registry = registry;
        _factory = factory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var services = _registry.GetCombinedConfig();
        var results = new Dictionary<string, HealthInfo>();

        foreach (var service in services)
        {
            try
            {
                var url = $"{service.DevUrl.TrimEnd('/')}{service.HealthEndpoint}";
                using var client = _factory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                using var response = await client.GetAsync(url, cancellationToken);
                results[service.Id] = new HealthInfo(response.IsSuccessStatusCode, (int)response.StatusCode, url);
            }
            catch (Exception ex)
            {
                results[service.Id] = new HealthInfo(false, 0, $"{service.DevUrl}{service.HealthEndpoint}", ex.Message);
            }
        }

        var allHealthy = results.Values.All(r => r.Healthy);
        var data = results.ToDictionary(kv => kv.Key, kv => (object)kv.Value);

        return allHealthy
            ? HealthCheckResult.Healthy("Tous les services aval sont joignables", data: data)
            : HealthCheckResult.Degraded("Certains services aval sont indisponibles", data: data);
    }

    private sealed record HealthInfo(bool Healthy, int StatusCode, string Url, string? Error = null);
}
