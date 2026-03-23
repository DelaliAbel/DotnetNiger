using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Community.Infrastructure.External;

/// <summary>
/// Implementation of IIdentityApiClient using typed HttpClient
/// Configured in DI with base address from appsettings "IdentityApi:BaseUrl"
/// </summary>
public class IdentityApiClient : IIdentityApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityApiClient> _logger;

    public IdentityApiClient(HttpClient httpClient, ILogger<IdentityApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get user details by ID from Identity service
    /// </summary>
    public async Task<UserDto?> GetUserAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/users/{userId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user {UserId}: {StatusCode}", userId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var user = JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return user;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error getting user {UserId}", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting user {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Get all roles for a user from Identity service
    /// </summary>
    public async Task<RoleDto[]> GetUserRolesAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/users/{userId}/roles", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get roles for user {UserId}: {StatusCode}", userId, response.StatusCode);
                return Array.Empty<RoleDto>();
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var roles = JsonSerializer.Deserialize<RoleDto[]>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return roles ?? Array.Empty<RoleDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error getting roles for user {UserId}", userId);
            return Array.Empty<RoleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting roles for user {UserId}", userId);
            return Array.Empty<RoleDto>();
        }
    }

    /// <summary>
    /// Validate if a JWT token is still valid on Identity service
    /// </summary>
    public async Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/validate-token")
            {
                Content = JsonContent.Create(new { token })
            };

            var response = await _httpClient.SendAsync(request, ct);
            var isValid = response.IsSuccessStatusCode;

            if (!isValid)
            {
                _logger.LogDebug("Token validation failed: {StatusCode}", response.StatusCode);
            }

            return isValid;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error validating token");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating token");
            return false;
        }
    }

    /// <summary>
    /// Check if Identity service is reachable via ping endpoint
    /// </summary>
    public async Task<bool> IsReachableAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/diagnostics/ping", ct);
            var isReachable = response.IsSuccessStatusCode;

            if (!isReachable)
            {
                _logger.LogWarning("Identity service unreachable: {StatusCode}", response.StatusCode);
            }

            return isReachable;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Identity service connection failed");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking Identity service");
            return false;
        }
    }

    /// <summary>
    /// Get health status from Identity service
    /// </summary>
    public async Task<HealthCheckDto?> GetHealthAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/health", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Health check failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var health = JsonSerializer.Deserialize<HealthCheckDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return health;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Health check HTTP error");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting health status");
            return null;
        }
    }
}

/// <summary>
/// DTO for user information from Identity service
/// </summary>
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for role information from Identity service
/// </summary>
public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// DTO for health check response
/// </summary>
public class HealthCheckDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Version { get; set; }
}
