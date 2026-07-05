using System.Text.Json.Serialization;

namespace DotnetNiger.Identity.Application.DTOs.Responses;

/// <summary>Résultat d'une vérification de santé d'un service.</summary>
public record HealthCheckResultDto
{
    [JsonPropertyName("isHealthy")]
    public bool IsHealthy { get; init; }
}
