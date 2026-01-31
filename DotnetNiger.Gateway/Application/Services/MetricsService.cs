using DotnetNiger.Gateway.Application.Services.Interfaces;

namespace DotnetNiger.Gateway.Application.Services;

/// <summary>
/// Collecte les métriques
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
    }

    public void RecordRequest(string endpoint, string method)
    {
        _logger.LogInformation("Request recorded: {Method} {Endpoint}", method, endpoint);
    }

    public void RecordResponseTime(string endpoint, long milliseconds)
    {
        _logger.LogInformation("Response time: {Endpoint} - {Ms}ms", endpoint, milliseconds);
    }

    public void RecordError(string endpoint, string errorType)
    {
        _logger.LogError("Error recorded: {Endpoint} - {ErrorType}", endpoint, errorType);
    }
}
