namespace DotnetNiger.Identity.Application.DTOs.Responses;

/// <summary>Réponse contenant les données d'un service externe.</summary>
public record ExternalServiceResponse(
    Guid Id,
    Guid TenantId,
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

/// <summary>Résultat de recherche d'un service externe.</summary>
public record ServiceLookupResult(string BaseUrl);
