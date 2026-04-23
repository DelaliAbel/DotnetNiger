using DotnetNiger.Community.Application.Services.Interfaces;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace DotnetNiger.Community.Application.Services;

public class IdentityApiClient : IIdentityApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public IdentityApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
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

    public async Task<bool> AssignMemberRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var provisioningKey = _configuration["IdentityApi:ProvisioningApiKey"];
        if (string.IsNullOrWhiteSpace(provisioningKey))
        {
            return false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/internal/assign-member-role")
        {
            Content = JsonContent.Create(new { UserId = userId })
        };

        request.Headers.Add("X-Internal-Key", provisioningKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
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
