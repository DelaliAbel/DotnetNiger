namespace DotnetNiger.Domain.DTOs.Responses;

public record TwoFactorRequiredResponse(
    bool RequiresTwoFactor,
    string ChallengeToken,
    string? TwoFactorType = "authenticator");
