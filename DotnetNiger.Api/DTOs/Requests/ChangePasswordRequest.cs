using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required] string NewPassword);
