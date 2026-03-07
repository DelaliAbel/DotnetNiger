namespace DotnetNiger.Identity.Application.DTOs.Requests;

public class SetRolePermissionsRequest
{
    public List<Guid> PermissionIds { get; set; } = new();
}
