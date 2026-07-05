using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête d'enregistrement d'un service externe.</summary>
public record RegisterExternalServiceRequest(
    [Required][StringLength(200, MinimumLength = 1)] string Name,
    [Required][RegularExpression(@"^[a-z0-9]+(-[a-z0-9]+)*$", ErrorMessage = "Slug must be lowercase alphanumeric with hyphens")] string Slug,
    [Required][Url] string BaseUrl,
    string? Description,
    string? HealthEndpoint);

/// <summary>Requête de mise à jour d'un service externe.</summary>
public record UpdateExternalServiceRequest(
    [Url] string? BaseUrl,
    string? Description,
    string? HealthEndpoint);
