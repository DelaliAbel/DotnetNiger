using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de création d'une clé API pour un tenant.</summary>
public record CreateTenantApiKeyRequest(
    [Required][StringLength(100, MinimumLength = 1)] string Name,
    string? Scopes,
    DateTime? ExpiresAt);
