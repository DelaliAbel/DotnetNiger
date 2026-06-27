using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetNiger.Gateway.Services;

/// <summary>Service d'arrière-plan qui vérifie périodiquement l'état de santé des services externes enregistrés.</summary>
public class ExternalServiceHealthService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExternalServiceHealthService> _logger;
    private readonly string _identityBaseUrl;
    private readonly int _intervalSeconds;
    private readonly int _maxFailures;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly string _internalApiKey;

    /// <summary>Initialise le service avec la configuration et les dépendances.</summary>
    public ExternalServiceHealthService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<ExternalServiceHealthService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _identityBaseUrl = (configuration["DeveloperPortal:IdentityBaseUrl"]
            ?? "http://localhost:5075").TrimEnd('/');
        _intervalSeconds = int.TryParse(
            configuration["DeveloperPortal:HealthCheckIntervalSeconds"], out var s) ? s : 30;
        _maxFailures = int.TryParse(
            configuration["DeveloperPortal:MaxHealthCheckFailures"], out var f) ? f : 3;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _internalApiKey = configuration["DeveloperPortal:InternalApiKey"] ?? "";
    }

    /// <summary>Boucle principale du service : vérifie tous les services à intervalle régulier.</summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExternalServiceHealthService started (interval: {Interval}s)", _intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAllServicesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in external service health check cycle");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task CheckAllServicesAsync(CancellationToken ct)
    {
        List<ExternalServiceHealthDto> services;
        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            if (!string.IsNullOrEmpty(_internalApiKey))
                client.DefaultRequestHeaders.Add("X-Internal-Key", _internalApiKey);
            var response = await client.GetAsync(
                $"{_identityBaseUrl}/api/v1/external-services/_internal/active", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch active external services (status {Status})",
                    response.StatusCode);
                return;
            }

            services = await response.Content
                .ReadFromJsonAsync<List<ExternalServiceHealthDto>>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct)
                ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to fetch active external services list from {Url}: {Message}",
                $"{_identityBaseUrl}/api/v1/external-services/_internal/active",
                ex.InnerException?.Message ?? ex.Message);
            return;
        }

        foreach (var service in services)
        {
            if (ct.IsCancellationRequested) break;

            var isHealthy = await CheckSingleServiceAsync(service, ct);
            await ReportHealthResultAsync(service, isHealthy, ct);
        }
    }

    private async Task<bool> CheckSingleServiceAsync(ExternalServiceHealthDto service, CancellationToken ct)
    {
        try
        {
            var healthUrl = $"{service.BaseUrl.TrimEnd('/')}/{service.HealthEndpoint.TrimStart('/')}";
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            using var response = await client.GetAsync(healthUrl, ct);

            var healthy = response.IsSuccessStatusCode;

            if (!healthy)
                _logger.LogWarning("Health check failed for {Slug} ({Name}): {Status}",
                    service.Slug, service.Name, response.StatusCode);

            return healthy;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Health check failed for {Slug} ({Name}): {Message}",
                service.Slug, service.Name,
                ex.InnerException?.Message ?? ex.Message);
            return false;
        }
    }

    private async Task ReportHealthResultAsync(ExternalServiceHealthDto service, bool isHealthy, CancellationToken ct)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.Add("X-Internal-Key", _internalApiKey);

            var payload = new HealthReportRequest { IsHealthy = isHealthy };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                $"{_identityBaseUrl}/api/v1/external-services/_internal/{service.Id}/health-result",
                content,
                ct);

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Failed to report health result for service {Id}: {Status}",
                    service.Id, response.StatusCode);

            if (!isHealthy)
            {
                var cacheKey = $"ext:{service.Slug}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to report health result for service {Id}", service.Id);
        }
    }

    private sealed record ExternalServiceHealthDto(
        Guid Id,
        string Name,
        string Slug,
        string BaseUrl,
        string HealthEndpoint);

    private sealed record HealthReportRequest
    {
        [JsonPropertyName("isHealthy")]
        public bool IsHealthy { get; init; }
    }
}
