using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de création d'un rôle.</summary>
public record CreateRoleRequest(
    [Required] string Name,
    string? Description,
    [Required] Guid TenantId);

/// <summary>Requête de mise à jour d'un rôle.</summary>
public record UpdateRoleRequest(
    string? Description);
