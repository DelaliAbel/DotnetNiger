using System.Net.Http.Json;
using DotnetNiger.Identity.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Identity.Infrastructure.External;

public class CommunityProvisioningClient : ICommunityProvisioningClient
{
    private const string ProvisioningEndpointPath = "api/v1/members/internal/provision";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CommunityProvisioningClient> _logger;

    public CommunityProvisioningClient(HttpClient httpClient, IConfiguration configuration, ILogger<CommunityProvisioningClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ProvisionPendingMemberAsync(Guid userId, string fullName, CancellationToken ct = default)
    {
        var provisioningKey = _configuration["CommunityApi:ProvisioningApiKey"];
        if (string.IsNullOrWhiteSpace(provisioningKey))
        {
            throw new InvalidOperationException("CommunityApi:ProvisioningApiKey is not configured.");
        }

        var provisioningUri = BuildProvisioningUri();

        using var request = new HttpRequestMessage(HttpMethod.Post, provisioningUri)
        {
            Content = JsonContent.Create(new
            {
                UserId = userId,
                FullName = fullName
            })
        };

        request.Headers.Add("X-Internal-Key", provisioningKey);

        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Community pending-member provisioning failed for UserId {UserId}. Status={StatusCode} Payload={Payload}", userId, (int)response.StatusCode, payload);
            throw new InvalidOperationException("Community pending-member provisioning failed.");
        }
    }

    private Uri BuildProvisioningUri()
    {
        var configuredBaseUrl = _configuration["CommunityApi:BaseUrl"]?.Trim();
        var baseUrl = string.IsNullOrWhiteSpace(configuredBaseUrl)
            ? _httpClient.BaseAddress?.ToString()
            : configuredBaseUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("CommunityApi:BaseUrl is not configured. Provide an absolute HTTP/HTTPS URL.");
        }

        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += "/";
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException($"Invalid CommunityApi:BaseUrl '{baseUrl}'. Use an absolute HTTP/HTTPS URL (e.g. http://localhost:5269/).");
        }

        if (!Uri.TryCreate(baseUri, ProvisioningEndpointPath, out var provisioningUri))
        {
            throw new InvalidOperationException("Failed to build Community provisioning endpoint URI.");
        }

        return provisioningUri;
    }
}
