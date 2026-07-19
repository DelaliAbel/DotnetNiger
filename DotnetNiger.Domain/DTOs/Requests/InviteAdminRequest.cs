using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public record InviteAdminRequest(
    [Required][EmailAddress] string Email,
    [Required] string Role);
