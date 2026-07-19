namespace DotnetNiger.Domain.DTOs.Responses;

public record TwoFactorChallenge(
    Guid UserId,
    string Email,
    DateTime ExpiresAt);
