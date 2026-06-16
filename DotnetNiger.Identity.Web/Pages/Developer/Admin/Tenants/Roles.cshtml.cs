using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin;

[Authorize(Roles = "Admin")]
public class RolesModel : BasePageModel
{
    public RolesModel(IHttpClientFactory http, IConfiguration config)
        : base(http, config) { }

    [BindProperty(SupportsGet = true)]
    public Guid TenantId { get; set; }

    public List<RoleItem> Roles { get; set; } = [];
    public List<PermissionGroup> PermissionGroups { get; set; } = [];
    public HashSet<Guid> RolePermissionIds { get; set; } = [];

    [BindProperty]
    public CreateRoleInput CreateInput { get; set; } = new();

    [BindProperty]
    public EditRoleInput EditInput { get; set; } = new();

    [BindProperty]
    public List<Guid> SelectedPermissionIds { get; set; } = [];

    public List<UserItem> AllUsers { get; set; } = [];

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        var result = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/{TenantId}/roles", new
        {
            name = CreateInput.Name,
            description = CreateInput.Description ?? "",
            tenantId = TenantId
        });

        if (result.Success)
        {
            SetMessage("Rôle créé.");
            CreateInput = new CreateRoleInput();
        }

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAssignPermissionsAsync(Guid roleId)
    {
        var result = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/{TenantId}/permissions/assign", new
        {
            roleId,
            permissionIds = SelectedPermissionIds
        });

        if (result.Success) SetMessage("Permissions mises à jour.");
        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid roleId)
    {
        var deleted = await DeleteAsync($"{GetIdentityUrl()}/api/v1/{TenantId}/roles/{roleId}");
        if (deleted) SetMessage("Rôle supprimé.");
        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(Guid roleId)
    {
        var ok = await PutAsync($"{GetIdentityUrl()}/api/v1/{TenantId}/roles/{roleId}", new
        {
            name = EditInput.Name,
            description = EditInput.Description ?? ""
        });

        if (ok) SetMessage("Rôle mis à jour.");
        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAddUserToRoleAsync(Guid roleId, Guid userId)
    {
        var result = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/{TenantId}/roles/{roleId}/users/{userId}");
        if (result.Success) SetMessage("Utilisateur ajouté au rôle.");
        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveUserFromRoleAsync(Guid roleId, Guid userId)
    {
        var deleted = await DeleteAsync($"{GetIdentityUrl()}/api/v1/{TenantId}/roles/{roleId}/users/{userId}");
        if (deleted) SetMessage("Utilisateur retiré du rôle.");
        await LoadDataAsync();
        return Page();
    }

    private async Task LoadDataAsync()
    {
        var identityUrl = GetIdentityUrl();
        var rolesData = await GetWithStatusAsync<PaginatedResponse<RoleItem>>($"{identityUrl}/api/v1/{TenantId}/roles?page={CurrentPage}&pageSize={PageSize}");
        Roles = rolesData.Data?.Items ?? [];
        TotalCount = rolesData.Data?.TotalCount ?? 0;

        var permData = await GetAsync<List<PermissionGroup>>($"{identityUrl}/api/v1/{TenantId}/permissions/grouped");
        PermissionGroups = permData ?? [];

        var usersData = await GetAsync<PaginatedResponse<UserItem>>($"{identityUrl}/api/v1/{TenantId}/users?pageSize=100");
        AllUsers = usersData?.Items ?? [];
    }
}
