using Microsoft.AspNetCore.Authorization;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin;

[Authorize(Roles = "Admin")]
public class IndexModel : BasePageModel
{
    public IndexModel(IHttpClientFactory http, IConfiguration config)
        : base(http, config) { }

    public AdminStats Stats { get; set; } = new();

    public async Task OnGetAsync()
    {
        var s = await GetAsync<AdminStats>($"{GetIdentityUrl()}/api/v1/admin/stats");
        if (s != null) Stats = s;
    }
}
