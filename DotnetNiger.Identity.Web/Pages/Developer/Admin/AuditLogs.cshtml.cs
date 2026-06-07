using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin;

[Authorize(Roles = "Admin")]
public class AuditLogsModel : PageModel
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public AuditLogsModel(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public List<AuditLogItem> Logs { get; set; } = [];
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; } = 1;
    public string Message { get; set; } = "";
    public bool IsError { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? EntityTypeFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ActionFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    public async Task OnGetAsync(int page = 1)
    {
        CurrentPage = Math.Max(1, page);
        await LoadLogsAsync();
    }

    private async Task LoadLogsAsync()
    {
        var identityUrl = _config["Identity:BaseUrl"]?.TrimEnd('/');
        var client = _http.CreateClient();
        var token = await HttpContext.GetTokenAsync("access_token");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var query = $"?page={CurrentPage}&pageSize=50";
            if (!string.IsNullOrEmpty(EntityTypeFilter))
                query += $"&entityType={Uri.EscapeDataString(EntityTypeFilter)}";
            if (!string.IsNullOrEmpty(ActionFilter))
                query += $"&action={Uri.EscapeDataString(ActionFilter)}";
            if (FromDate.HasValue)
                query += $"&from={FromDate.Value:yyyy-MM-dd}";
            if (ToDate.HasValue)
                query += $"&to={ToDate.Value:yyyy-MM-dd}";

            var resp = await client.GetAsync($"{identityUrl}/api/v1/admin/audit-logs{query}");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var paginated = JsonSerializer.Deserialize<AuditLogPaginatedResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Logs = paginated?.Items ?? [];
                TotalCount = paginated?.TotalCount ?? 0;
            }
        }
        catch (Exception ex)
        {
            Message = $"Erreur : {ex.Message}";
            IsError = true;
        }
    }
}

public class AuditLogPaginatedResponse
{
    public List<AuditLogItem> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class AuditLogItem
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid UserId { get; set; }
    public string EntityType { get; set; } = "";
    public Guid EntityId { get; set; }
    public string Action { get; set; } = "";
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
