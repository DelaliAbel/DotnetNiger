using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de création d'un tenant.</summary>
public record CreateTenantRequest(
    [Required] string Name,
    [Required] string Slug,
    string? Description);

/// <summary>Requête de mise à jour d'un tenant.</summary>
public record UpdateTenantRequest(
    string? Name,
    string? Description,
    bool? IsActive);
