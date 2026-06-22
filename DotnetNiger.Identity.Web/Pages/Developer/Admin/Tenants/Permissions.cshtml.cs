using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Identity.Web.Infrastructure;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin.Tenants;

[Authorize(Roles = "Admin,SuperAdmin")]
public class PermissionsModel : BasePageModel
{
    public PermissionsModel(IHttpClientFactory http, IConfiguration config)
        : base(http, config) { }

    [BindProperty(SupportsGet = true)]
    public Guid TenantId { get; set; }

    public List<PermissionGroupItem> GroupedPermissions { get; set; } = [];
    public List<PermissionItem> AllPermissions { get; set; } = [];
    public List<RoleItem> Roles { get; set; } = [];

    [BindProperty]
    public CreatePermissionInput CreateInput { get; set; } = new();

    [BindProperty]
    public AssignPermissionsInput AssignInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadAllAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAllAsync();
            return Page();
        }

        var result = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/{TenantId}/permissions", new
        {
            name = CreateInput.Name,
            category = CreateInput.Category,
            tenantId = TenantId
        });

        if (result.Success)
        {
            SetMessage("Permission créée.");
            CreateInput = new CreatePermissionInput();
        }

        await LoadAllAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid permissionId)
    {
        var deleted = await DeleteAsync($"{GetIdentityUrl()}/api/v1/{TenantId}/permissions/{permissionId}");
        if (deleted) SetMessage("Permission supprimée.");
        await LoadAllAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAssignAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAllAsync();
            return Page();
        }

        var selectedIds = Request.Form["AssignInput.PermissionIds"]
            .SelectMany(v => (v ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();

        var result = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/{TenantId}/permissions/assign", new
        {
            roleId = AssignInput.RoleId,
            permissionIds = selectedIds
        });

        if (result.Success) SetMessage("Permissions assignées au rôle.");
        await LoadAllAsync();
        return Page();
    }

    private async Task LoadAllAsync()
    {
        var identityUrl = GetIdentityUrl();
        var tasks = new List<Task>
        {
            LoadPermissionsAsync(identityUrl),
            LoadRolesAsync(identityUrl)
        };

        await Task.WhenAll(tasks);
    }

    private async Task LoadPermissionsAsync(string identityUrl)
    {
        var grouped = await GetAsync<List<PermissionGroupItem>>($"{identityUrl}/api/v1/{TenantId}/permissions/grouped");
        if (grouped != null)
        {
            GroupedPermissions = grouped;
            AllPermissions = grouped.SelectMany(g => g.Permissions).ToList();
            TotalCount = AllPermissions.Count;
        }
    }

    private async Task LoadRolesAsync(string identityUrl)
    {
        var paginated = await GetAsync<RoleListResponse>($"{identityUrl}/api/v1/{TenantId}/roles?pageSize=100");
        Roles = paginated?.Items ?? [];
    }
}

public class PermissionItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public Guid TenantId { get; set; }
}

public class PermissionGroupItem
{
    public string Category { get; set; } = "";
    public List<PermissionItem> Permissions { get; set; } = [];
}

public class RoleItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}

public class RoleListResponse
{
    public List<RoleItem> Items { get; set; } = [];
    public int TotalCount { get; set; }
}

public class CreatePermissionInput
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
}

public class AssignPermissionsInput
{
    public Guid RoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = [];
}
