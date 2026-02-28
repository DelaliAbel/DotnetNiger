namespace DotnetNiger.Gateway.Infrastructure.HttpClients;

/// <summary>
/// Client pour communiquer avec le service Identity
/// </summary>
public class IdentityApiClient : ApiClientBase, IIdentityApiClient
{
    private const string ClusterName = "identity-cluster";

    public IdentityApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<IdentityApiClient> logger)
        : base(httpClient, configuration, logger)
    {
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return false;

        var url = $"{baseAddress}/api/tokens/validate";
        var response = await HttpClient.PostAsync(
            url,
            new StringContent(System.Text.Json.JsonSerializer.Serialize(new { token }),
            System.Text.Encoding.UTF8,
            "application/json"));

        return response.IsSuccessStatusCode;
    }

    public async Task<UserInfoResponse?> GetCurrentUserAsync(string token)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/users/me";
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var result = await GetAsync<UserInfoResponse>(url);
        HttpClient.DefaultRequestHeaders.Authorization = null;

        return result;
    }

    public async Task<UserInfoResponse?> GetUserByIdAsync(string userId)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/users/{userId}";
        return await GetAsync<UserInfoResponse>(url);
    }

    public async Task<List<RoleResponse>?> GetRolesAsync()
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/roles";
        return await GetAsync<List<RoleResponse>>(url);
    }

    public async Task<AuthResponse?> AuthenticateAsync(LoginRequest request)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/auth/login";
        return await PostAsync<AuthResponse>(url, request);
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/auth/register";
        return await PostAsync<AuthResponse>(url, request);
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/tokens/refresh";
        return await PostAsync<AuthResponse>(url, new { refreshToken });
    }
}
