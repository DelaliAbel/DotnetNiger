using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin.Tenants;

[Authorize(Roles = "Admin")]
public class LoginHistoryModel : PageModel
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public LoginHistoryModel(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    [BindProperty(SupportsGet = true)]
    public Guid TenantId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

    public List<LoginHistoryEntry> Entries { get; set; } = [];
    public string Message { get; set; } = "";
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        await LoadEntriesAsync();
    }

    private async Task LoadEntriesAsync()
    {
        var identityUrl = _config["Identity:BaseUrl"]?.TrimEnd('/');
        var client = _http.CreateClient();
        var token = await HttpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var resp = await client.GetAsync(
                $"{identityUrl}/api/v1/admin/tenants/{TenantId}/login-history?page={CurrentPage}&pageSize={PageSize}");

            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                TotalCount = root.GetProperty("totalCount").GetInt32();

                var items = new List<LoginHistoryEntry>();
                foreach (var item in root.GetProperty("items").EnumerateArray())
                {
                    items.Add(new LoginHistoryEntry
                    {
                        Timestamp = item.GetProperty("createdAt").GetDateTime(),
                        Email = item.GetProperty("email").GetString() ?? "",
                        IpAddress = item.GetProperty("ipAddress").GetString() ?? "",
                        UserAgent = item.GetProperty("userAgent").GetString() ?? "",
                        Success = item.GetProperty("success").GetBoolean(),
                        FailureReason = item.TryGetProperty("failureReason", out var fr) ? fr.GetString() : null
                    });
                }
                Entries = items;
            }
        }
        catch (Exception ex)
        {
            Message = $"Erreur de chargement : {ex.Message}";
            IsError = true;
        }
    }
}

public class LoginHistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string Email { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
}
