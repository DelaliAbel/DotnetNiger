using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public record InviteAdminRequest(
    [Required][EmailAddress] string Email,
    [Required] string Role);
