using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public record CreateRoleRequest(
    [Required] string Name,
    string? Description);

public record UpdateRoleRequest(
    string? Description);
