using System.Security.Claims;

namespace DotnetNiger.Identity.Application.DTOs.Responses;

public class TokenExchangeResult
{
    public ClaimsPrincipal? Principal { get; init; }
    public string? Error { get; init; }
    public bool RequiresTwoFactor { get; init; }
    public string? ChallengeToken { get; init; }

    public static TokenExchangeResult Success(ClaimsPrincipal principal) => new() { Principal = principal };
    public static TokenExchangeResult Failure(string error) => new() { Error = error };
    public static TokenExchangeResult TwoFactorRequired(string challengeToken) => new() { RequiresTwoFactor = true, ChallengeToken = challengeToken };
}
