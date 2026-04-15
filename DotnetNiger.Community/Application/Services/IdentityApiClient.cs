using DotnetNiger.Community.Application.Services.Interfaces;
using System.Net.Http.Json;

namespace DotnetNiger.Community.Application.Services;

public class IdentityApiClient : IIdentityApiClient
{
    private readonly HttpClient _httpClient;

    public IdentityApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string BaseUrl => _httpClient.BaseAddress?.ToString() ?? string.Empty;

    public async Task<bool> IsReachableAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/v1/diagnostics/health", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<Guid?> RegisterAsync(IdentityRegisterRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/v1/auth/register", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<IdentityApiSuccessResponse<IdentityAuthPayload>>(cancellationToken: cancellationToken);
        return payload?.Data?.User?.Id;
    }
}

file sealed class IdentityApiSuccessResponse<T>
{
    public T? Data { get; set; }
}

file sealed class IdentityAuthPayload
{
    public IdentityUserPayload? User { get; set; }
}

file sealed class IdentityUserPayload
{
    public Guid Id { get; set; }
}
