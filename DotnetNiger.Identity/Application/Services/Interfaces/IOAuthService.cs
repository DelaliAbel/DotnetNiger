using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IOAuthService
{
    Task<IReadOnlyList<string>> GetEnabledProvidersAsync();
    Task<AuthResponse> ExchangeAccessTokenAsync(OAuthExchangeRequest request, CancellationToken ct = default);
}
