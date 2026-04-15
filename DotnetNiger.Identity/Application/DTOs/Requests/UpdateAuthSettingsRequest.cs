using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

public record UpdateAuthSettingsRequest
{
    [StringLength(64)]
    public string? DefaultRole { get; init; }

    [Range(1, 100)]
    public int? RegisterMaxAttempts { get; init; }

    [Range(1, 100)]
    public int? LoginMaxAttempts { get; init; }

    [Range(1, 100)]
    public int? PasswordResetMaxAttempts { get; init; }

    [Range(1, 120)]
    public int? RateLimitWindowMinutes { get; init; }
}
