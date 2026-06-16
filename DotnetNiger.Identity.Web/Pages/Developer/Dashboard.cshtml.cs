using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer;

[Authorize]
public class DashboardModel : BasePageModel
{
    public DashboardModel(IHttpClientFactory http, IConfiguration config, ILogger<DashboardModel> logger)
        : base(http, config, logger) { }

    public DashboardStats Stats { get; set; } = new();
    public List<LoginHistoryEntry> RecentLogins { get; set; } = [];
    public List<ServiceItem> RecentServices { get; set; } = [];
    public string LoginChartLabels { get; set; } = "[]";
    public string LoginChartData { get; set; } = "[]";

    public async Task OnGetAsync()
    {
        var identityUrl = GetIdentityUrl();
        if (string.IsNullOrEmpty(identityUrl)) return;

        var profile = await GetAsync<ProfileResponse>($"{identityUrl}/api/v1/profile");
        if (profile == null) return;

        Stats.TenantId = profile.TenantId?.ToString() ?? "";
        var tenant = await GetAsync<TenantItem>($"{identityUrl}/api/v1/admin/tenants/{profile.TenantId}");
        Stats.TenantName = tenant?.Name ?? profile.TenantId?.ToString() ?? "—";

        if (profile.TenantId == null) return;
        var tid = profile.TenantId.Value;

        var tasks = new List<Task>
        {
            LoadApiKeysCountAsync(identityUrl, tid),
            LoadServicesCountAsync(identityUrl),
            LoadUsersCountAsync(identityUrl, tid),
            LoadRolesCountAsync(identityUrl, tid),
            LoadLoginHistoryAsync(identityUrl),
            LoadRecentServicesAsync(identityUrl)
        };

        await Task.WhenAll(tasks);
    }

    private async Task LoadApiKeysCountAsync(string identityUrl, Guid tenantId)
    {
        var data = await GetAsync<CountResponse>($"{identityUrl}/api/v1/admin/tenants/{tenantId}/api-keys?pageSize=1");
        Stats.ActiveApiKeys = data?.TotalCount ?? 0;
    }

    private async Task LoadServicesCountAsync(string identityUrl)
    {
        var data = await GetAsync<CountResponse>($"{identityUrl}/api/v1/external-services?pageSize=1");
        Stats.ActiveServices = data?.TotalCount ?? 0;
    }

    private async Task LoadUsersCountAsync(string identityUrl, Guid tenantId)
    {
        var data = await GetAsync<CountResponse>($"{identityUrl}/api/v1/{tenantId}/users?pageSize=1");
        Stats.TotalUsers = data?.TotalCount ?? 0;
    }

    private async Task LoadRolesCountAsync(string identityUrl, Guid tenantId)
    {
        var data = await GetAsync<CountResponse>($"{identityUrl}/api/v1/{tenantId}/roles?pageSize=1");
        Stats.TotalRoles = data?.TotalCount ?? 0;
    }

    private async Task LoadLoginHistoryAsync(string identityUrl)
    {
        var paginated = await GetAsync<PaginatedResponse<LoginHistoryEntry>>($"{identityUrl}/api/v1/profile/login-history?pageSize=50");
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

    private async Task LoadRecentServicesAsync(string identityUrl)
    {
        var paginated = await GetAsync<PaginatedResponse<ServiceItem>>($"{identityUrl}/api/v1/external-services?pageSize=5");
        RecentServices = paginated?.Items ?? [];
    }
}

public class CountResponse
{
    public int TotalCount { get; set; }
}
