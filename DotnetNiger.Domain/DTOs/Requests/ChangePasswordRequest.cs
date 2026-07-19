using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required] string NewPassword);
