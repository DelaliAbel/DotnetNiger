using Microsoft.AspNetCore.Authorization;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer;

[Authorize]
public class SecuriteModel : BasePageModel
{
    public SecuriteModel(IHttpClientFactory http, IConfiguration config, ILogger<SecuriteModel> logger)
        : base(http, config, logger) { }

    public DateTime? LastLogin { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public List<LoginHistoryEntry> RecentLogins { get; set; } = [];
    public List<ActiveSession> ActiveSessions { get; set; } = [];

    public async Task OnGetAsync()
    {
        var identityUrl = GetIdentityUrl();
        if (string.IsNullOrEmpty(identityUrl)) return;

        var tasks = new List<Task>
        {
            LoadLoginHistoryAsync(identityUrl),
            LoadTwoFactorStatusAsync(identityUrl)
        };

        await Task.WhenAll(tasks);
    }

    private async Task LoadLoginHistoryAsync(string identityUrl)
    {
        var paginated = await GetAsync<PaginatedResponse<LoginHistoryEntry>>($"{identityUrl}/api/v1/profile/login-history?pageSize=20");
        var logins = paginated?.Items ?? [];

        RecentLogins = logins;
        LastLogin = logins.Where(l => l.Success).Select(l => l.Timestamp as DateTime?).FirstOrDefault();

        ActiveSessions = logins
            .Where(l => l.Success && !string.IsNullOrEmpty(l.UserAgent))
            .GroupBy(l => new { l.IpAddress, Agent = NormalizeUserAgent(l.UserAgent ?? "") })
            .Select(g => new ActiveSession
            {
                IpAddress = g.Key.IpAddress ?? "",
                UserAgent = g.Key.Agent,
                LastActivity = g.Max(x => x.Timestamp),
                DeviceName = GetDeviceName(g.Key.Agent),
                BrowserName = GetBrowserName(g.Key.Agent)
            })
            .OrderByDescending(s => s.LastActivity)
            .ToList();
    }

    private async Task LoadTwoFactorStatusAsync(string identityUrl)
    {
        var result = await GetAsync<TwoFactorStatusResponse>($"{identityUrl}/api/v1/profile/two-factor/status");
        if (result != null)
        {
            TwoFactorEnabled = result.TwoFactorEnabled;
            RecoveryCodesLeft = result.RecoveryCodesLeft;
        }
    }

    private static string NormalizeUserAgent(string ua)
    {
        if (ua.Length > 100) ua = ua[..100];
        return ua;
    }

    private static string GetDeviceName(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Inconnu";
        var ua = userAgent.ToLowerInvariant();
        if (ua.Contains("iphone") || ua.Contains("ipad")) return "Apple iOS";
        if (ua.Contains("android")) return "Android";
        if (ua.Contains("linux") && ua.Contains("android")) return "Android";
        if (ua.Contains("windows")) return "Windows";
        if (ua.Contains("macintosh") || ua.Contains("mac os")) return "macOS";
        if (ua.Contains("linux")) return "Linux";
        return "Autre";
    }

    private static string GetBrowserName(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "—";
        var ua = userAgent.ToLowerInvariant();
        if (ua.Contains("edg/") || ua.Contains("edge")) return "Edge";
        if (ua.Contains("chrome") && !ua.Contains("chromium")) return "Chrome";
        if (ua.Contains("firefox")) return "Firefox";
        if (ua.Contains("safari") && !ua.Contains("chrome")) return "Safari";
        if (ua.Contains("opera") || ua.Contains("opr/")) return "Opera";
        return "—";
    }
}


