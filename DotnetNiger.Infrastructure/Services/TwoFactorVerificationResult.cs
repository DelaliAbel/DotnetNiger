using System.Security.Claims;

namespace DotnetNiger.Infrastructure.Services;

public class TwoFactorVerificationResult
{
    public ClaimsPrincipal? Principal { get; init; }
    public bool RateLimited { get; init; }
    public string? Error { get; init; }

    public static TwoFactorVerificationResult Success(ClaimsPrincipal principal) => new() { Principal = principal };
    public static TwoFactorVerificationResult RateLimitedResult() => new() { RateLimited = true };
    public static TwoFactorVerificationResult Failure(string error) => new() { Error = error };
}
