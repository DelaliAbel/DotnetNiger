using System.Text.Json.Serialization;

namespace DotnetNiger.Domain.DTOs.Responses;

public record HealthCheckResultResponse
{
    [JsonPropertyName("isHealthy")]
    public bool IsHealthy { get; init; }
}
