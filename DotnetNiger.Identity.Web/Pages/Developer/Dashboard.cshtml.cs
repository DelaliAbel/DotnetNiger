using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetNiger.Identity.Web.Pages.Developer;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    private readonly ILogger<DashboardModel> _logger;

    public DashboardModel(IHttpClientFactory http, IConfiguration config, ILogger<DashboardModel> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public DashboardStats Stats { get; set; } = new();
    public List<LoginHistoryEntry> RecentLogins { get; set; } = [];
    public List<ServiceItem> RecentServices { get; set; } = [];
    public string LoginChartLabels { get; set; } = "[]";
    public string LoginChartData { get; set; } = "[]";

    private async Task<string?> GetTenantNameAsync(HttpClient client, string? identityUrl, Guid? tenantId)
    {
        if (tenantId == null) return null;
        try
        {
            var resp = await client.GetAsync($"{identityUrl}/api/v1/admin/tenants/{tenantId}");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("name").GetString();
            }
        }
        catch { }
        return null;
    }

    public async Task OnGetAsync()
    {
        var identityUrl = _config["Identity:BaseUrl"]?.TrimEnd('/');
        var client = _http.CreateClient();
        var token = await HttpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Guid? tenantId = null;

        try
        {
            var profileResp = await client.GetAsync($"{identityUrl}/api/v1/profile");
            if (profileResp.IsSuccessStatusCode)
            {
                var profile = JsonSerializer.Deserialize<ProfileJson>(
                    await profileResp.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (profile != null)
                {
                    tenantId = profile.TenantId;
                    Stats.TenantId = profile.TenantId?.ToString() ?? "";
                    Stats.TenantName = await GetTenantNameAsync(client, identityUrl, profile.TenantId) ?? profile.TenantId?.ToString() ?? "—";
                }
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to load profile"); }

        if (tenantId == null) return;

        var tasks = new List<Task>
        {
            LoadApiKeysCountAsync(client, identityUrl, tenantId.Value),
            LoadServicesCountAsync(client, identityUrl),
            LoadUsersCountAsync(client, identityUrl, tenantId.Value),
            LoadRolesCountAsync(client, identityUrl, tenantId.Value),
            LoadLoginHistoryAsync(client, identityUrl),
            LoadRecentServicesAsync(client, identityUrl)
        };

        await Task.WhenAll(tasks);
    }

    private async Task LoadApiKeysCountAsync(HttpClient client, string? identityUrl, Guid tenantId)
    {
        try
        {
            var resp = await client.GetAsync($"{identityUrl}/api/v1/admin/tenants/{tenantId}/api-keys?pageSize=1");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<DashboardPaginated>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Stats.ActiveApiKeys = data?.TotalCount ?? 0;
            }
        }
        catch { }
    }

    private async Task LoadServicesCountAsync(HttpClient client, string? identityUrl)
    {
        try
        {
            var resp = await client.GetAsync($"{identityUrl}/api/v1/external-services?pageSize=1");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<DashboardPaginated>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Stats.ActiveServices = data?.TotalCount ?? 0;
            }
        }
        catch { }
    }

    private async Task LoadUsersCountAsync(HttpClient client, string? identityUrl, Guid tenantId)
    {
        try
        {
            var resp = await client.GetAsync($"{identityUrl}/api/v1/{tenantId}/users?pageSize=1");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<DashboardPaginated>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Stats.TotalUsers = data?.TotalCount ?? 0;
            }
        }
        catch { }
    }

    private async Task LoadRolesCountAsync(HttpClient client, string? identityUrl, Guid tenantId)
    {
        try
        {
            var resp = await client.GetAsync($"{identityUrl}/api/v1/{tenantId}/roles?pageSize=1");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<DashboardPaginated>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Stats.TotalRoles = data?.TotalCount ?? 0;
            }
        }
        catch { }
    }

    private async Task LoadLoginHistoryAsync(HttpClient client, string? identityUrl)
    {
        try
        {
            var resp = await client.GetAsync($"{identityUrl}/api/v1/profile/login-history?pageSize=50");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var paginated = JsonSerializer.Deserialize<LoginHistoryPaginated>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var logins = paginated?.Items ?? [];

                RecentLogins = logins.Take(10).ToList();

                var last7Days = logins
                    .Where(l => l.Timestamp >= DateTime.UtcNow.AddDays(-7))
                    .GroupBy(l => l.Timestamp.Date)
                    .OrderBy(g => g.Key)
                    .ToList();

                var labels = new List<string>();
                var data = new List<int>();

                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.AddDays(-i).Date;
                    labels.Add(date.ToString("dd/MM"));
                    var count = last7Days.FirstOrDefault(g => g.Key == date)?.Count() ?? 0;
                    data.Add(count);
                }

                LoginChartLabels = JsonSerializer.Serialize(labels);
                LoginChartData = JsonSerializer.Serialize(data);

                Stats.TotalLogins = logins.Count;
                Stats.SuccessfulLogins = logins.Count(l => l.Success);
                Stats.FailedLogins = logins.Count(l => !l.Success);
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to load login history"); }
    }

    private async Task LoadRecentServicesAsync(HttpClient client, string? identityUrl)
    {
        try
        {
            var resp = await client.GetAsync($"{identityUrl}/api/v1/external-services?pageSize=5");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var paginated = JsonSerializer.Deserialize<PaginatedResponse<ServiceItem>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                RecentServices = paginated?.Items ?? [];
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to load recent services"); }
    }
}

public class DashboardPaginated
{
    public int TotalCount { get; set; }
}

public class DashboardStats
{
    public string TenantName { get; set; } = "—";
    public string TenantId { get; set; } = "";
    public int ActiveApiKeys { get; set; }
    public int ActiveServices { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRoles { get; set; }
    public int TotalLogins { get; set; }
    public int SuccessfulLogins { get; set; }
    public int FailedLogins { get; set; }
    public bool GatewayConnected { get; set; }
}

public class ProfileJson
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Guid? TenantId { get; set; }
    public List<string>? Roles { get; set; }
}
