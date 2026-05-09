using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Dtos.Requests;

public class AssignRoleRequest
{
    [Required]
    public string RoleName { get; set; } = string.Empty;
}
