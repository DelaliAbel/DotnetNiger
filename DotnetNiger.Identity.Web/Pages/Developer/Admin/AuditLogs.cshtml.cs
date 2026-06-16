using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin;

[Authorize(Roles = "Admin")]
public class AuditLogsModel : BasePageModel
{
    public AuditLogsModel(IHttpClientFactory http, IConfiguration config)
        : base(http, config) { }

    public List<AuditLogItem> Logs { get; set; } = [];

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
        var query = $"?page={CurrentPage}&pageSize=50";
        if (!string.IsNullOrEmpty(EntityTypeFilter))
            query += $"&entityType={Uri.EscapeDataString(EntityTypeFilter)}";
        if (!string.IsNullOrEmpty(ActionFilter))
            query += $"&action={Uri.EscapeDataString(ActionFilter)}";
        if (FromDate.HasValue)
            query += $"&from={FromDate.Value:yyyy-MM-dd}";
        if (ToDate.HasValue)
            query += $"&to={ToDate.Value:yyyy-MM-dd}";

        var result = await GetWithStatusAsync<AuditLogPaginatedResponse>($"{GetIdentityUrl()}/api/v1/admin/audit-logs{query}");
        Logs = result.Data?.Items ?? [];
        TotalCount = result.Data?.TotalCount ?? 0;
    }
}
