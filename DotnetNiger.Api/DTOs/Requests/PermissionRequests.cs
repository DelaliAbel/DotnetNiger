using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public record CreatePermissionRequest(
    [Required] string Name,
    [Required] string Category);

public record AssignPermissionsRequest(
    [Required] Guid RoleId,
    [Required] IList<Guid> PermissionIds);
