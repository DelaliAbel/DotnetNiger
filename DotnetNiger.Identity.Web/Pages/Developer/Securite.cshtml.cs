using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetNiger.Identity.Web.Pages.Developer;

[Authorize]
public class SecuriteModel : PageModel
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    private readonly ILogger<SecuriteModel> _logger;

    public SecuriteModel(IHttpClientFactory http, IConfiguration config, ILogger<SecuriteModel> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public DateTime? LastLogin { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public List<LoginHistoryEntry> RecentLogins { get; set; } = [];
    public List<ActiveSession> ActiveSessions { get; set; } = [];
    public string Message { get; set; } = "";
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        await LoadSecurityDataAsync();
    }

    private async Task LoadSecurityDataAsync()
    {
        var identityUrl = _config["Identity:BaseUrl"]?.TrimEnd('/');
        var client = _http.CreateClient();
        var token = await HttpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var tasks = new List<Task>
        {
            LoadLoginHistoryAsync(client, identityUrl),
            LoadTwoFactorStatusAsync(client, identityUrl)
        };

        await Task.WhenAll(tasks);
    }

    private async Task LoadLoginHistoryAsync(HttpClient client, string? identityUrl)
    {
        try
        {
            var response = await client.GetAsync($"{identityUrl}/api/v1/profile/login-history?pageSize=20");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var paginated = JsonSerializer.Deserialize<LoginHistoryPaginated>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                RecentLogins = paginated?.Items ?? [];

                LastLogin = RecentLogins
                    .Where(l => l.Success)
                    .Select(l => l.Timestamp as DateTime?)
                    .FirstOrDefault();

                ActiveSessions = RecentLogins
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
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load login history");
        }
    }

    private async Task LoadTwoFactorStatusAsync(HttpClient client, string? identityUrl)
    {
        try
        {
            var response = await client.GetAsync($"{identityUrl}/api/v1/profile/two-factor/status");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                TwoFactorEnabled = root.GetProperty("twoFactorEnabled").GetBoolean();
                RecoveryCodesLeft = root.GetProperty("recoveryCodesLeft").GetInt32();
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to load 2FA status");
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

public class ActiveSession
{
    public string IpAddress { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public DateTime LastActivity { get; set; }
    public string DeviceName { get; set; } = "";
    public string BrowserName { get; set; } = "";
}
