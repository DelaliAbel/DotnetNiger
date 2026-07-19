using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public record ExternalLoginRequest(
    [Required] string Provider,
    string? ReturnUrl);
