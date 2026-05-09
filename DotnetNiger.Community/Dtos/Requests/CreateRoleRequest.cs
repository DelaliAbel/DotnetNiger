using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Dtos.Requests;

public class CreateRoleRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
