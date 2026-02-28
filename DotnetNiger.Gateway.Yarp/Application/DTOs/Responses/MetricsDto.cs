namespace DotnetNiger.Gateway.Application.DTOs.Responses;

/// <summary>
/// DTO pour les métriques
/// </summary>
public class MetricsDto
{
    public long TotalRequests { get; set; }
    public long SuccessfulRequests { get; set; }
    public long FailedRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public Dictionary<string, long> RequestsByEndpoint { get; set; } = new();
}
