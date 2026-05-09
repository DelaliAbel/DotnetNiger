using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Dtos.Requests;

public class AssignPermissionRequest
{
    [Required]
    public Guid PermissionId { get; set; }
}
