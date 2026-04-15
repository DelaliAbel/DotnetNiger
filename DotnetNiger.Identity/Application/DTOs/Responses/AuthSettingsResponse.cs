namespace DotnetNiger.Identity.Application.DTOs.Responses;

public record AuthSettingsResponse
{
    public string DefaultRole { get; init; } = "Member";
    public int RegisterMaxAttempts { get; init; }
    public int LoginMaxAttempts { get; init; }
    public int PasswordResetMaxAttempts { get; init; }
    public int RateLimitWindowMinutes { get; init; }
}
