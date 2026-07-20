using System.Security.Claims;

namespace DotnetNiger.Domain.DTOs.Responses;

public class TokenExchangeResult
{
    public ClaimsPrincipal? Principal { get; init; }
    public string? Error { get; init; }

    public static TokenExchangeResult Success(ClaimsPrincipal principal) => new() { Principal = principal };
    public static TokenExchangeResult Failure(string error) => new() { Error = error };
}
