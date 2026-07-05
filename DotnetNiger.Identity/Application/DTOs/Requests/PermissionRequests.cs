using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de création d'une permission.</summary>
public record CreatePermissionRequest(
    [Required] string Name,
    [Required] string Category,
    [Required] Guid TenantId);

/// <summary>Requête d'attribution de permissions à un rôle.</summary>
public record AssignPermissionsRequest(
    [Required] Guid RoleId,
    [Required] IList<Guid> PermissionIds);
