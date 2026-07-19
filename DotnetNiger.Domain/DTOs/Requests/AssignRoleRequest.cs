using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public record AssignRoleRequest(
    [Required] string RoleName);
