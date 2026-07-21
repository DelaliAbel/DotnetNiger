using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public record AssignRoleRequest(
    [Required] string RoleName);
