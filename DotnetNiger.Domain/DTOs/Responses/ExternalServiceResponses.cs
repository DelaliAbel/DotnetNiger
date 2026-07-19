namespace DotnetNiger.Domain.DTOs.Responses;

public record ExternalServiceResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string BaseUrl,
    string HealthEndpoint,
    bool IsActive,
    string Status,
    DateTime? LastHealthCheckAt,
    int HealthCheckFailures,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ServiceLookupResult(string BaseUrl);
