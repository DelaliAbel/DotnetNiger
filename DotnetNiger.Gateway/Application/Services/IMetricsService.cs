namespace DotnetNiger.Gateway.Application.Services;

/// <summary>
/// Interface pour le service de métriques
/// </summary>
public interface IMetricsService
{
    void RecordRequest(string endpoint, string method);
    void RecordResponseTime(string endpoint, long milliseconds);
    void RecordError(string endpoint, string errorType);
}
