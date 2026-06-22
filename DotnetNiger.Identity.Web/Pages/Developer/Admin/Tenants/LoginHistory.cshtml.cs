using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin.Tenants;

[Authorize(Roles = "Admin,SuperAdmin")]
public class LoginHistoryModel : BasePageModel
{
    public LoginHistoryModel(IHttpClientFactory http, IConfiguration config)
        : base(http, config) { }

    [BindProperty(SupportsGet = true)]
    public Guid TenantId { get; set; }

    public List<LoginHistoryEntry> Entries { get; set; } = [];

    public async Task OnGetAsync()
    {
        var data = await GetWithStatusAsync<PaginatedResponse<LoginHistoryEntry>>(
            $"{GetIdentityUrl()}/api/v1/admin/tenants/{TenantId}/login-history?page={CurrentPage}&pageSize={PageSize}");
        Entries = data.Data?.Items ?? [];
        TotalCount = data.Data?.TotalCount ?? 0;
    }
}
