using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin;

[Authorize(Roles = "Admin")]
public class TenantsModel : BasePageModel
{
    public TenantsModel(IHttpClientFactory http, IConfiguration config)
        : base(http, config) { }

    public List<TenantItem> Tenants { get; set; } = [];

    [BindProperty]
    public CreateTenantInput CreateInput { get; set; } = new();

    [BindProperty]
    public EditTenantInput EditInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadTenantsAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadTenantsAsync();
            return Page();
        }

        var result = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/admin/tenants", new
        {
            name = CreateInput.Name,
            slug = CreateInput.Slug,
            description = CreateInput.Description ?? ""
        });

        if (result.Success)
        {
            var defaultEmail = $"admin@{CreateInput.Slug}.dotnetniger.com";
            SetMessage($"Tenant créé avec succès ! Compte admin par défaut : {defaultEmail} (mot de passe configuré dans Identity)");
            CreateInput = new CreateTenantInput();
        }

        await LoadTenantsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(Guid tenantId)
    {
        if (!ModelState.IsValid)
        {
            await LoadTenantsAsync();
            return Page();
        }

        var body = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(EditInput.Name)) body["name"] = EditInput.Name;
        if (EditInput.Description != null) body["description"] = EditInput.Description;

        var ok = await PutAsync($"{GetIdentityUrl()}/api/v1/admin/tenants/{tenantId}", body);
        if (ok) SetMessage("Tenant mis à jour.");

        await LoadTenantsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(Guid tenantId, bool isActive)
    {
        var ok = await PutAsync($"{GetIdentityUrl()}/api/v1/admin/tenants/{tenantId}", new { isActive = !isActive });
        if (ok) SetMessage(isActive ? "Tenant désactivé." : "Tenant activé.");
        await LoadTenantsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid tenantId)
    {
        var deleted = await DeleteAsync($"{GetIdentityUrl()}/api/v1/admin/tenants/{tenantId}");
        if (deleted) SetMessage("Tenant supprimé.");
        await LoadTenantsAsync();
        return Page();
    }

    private async Task LoadTenantsAsync()
    {
        var (data, ok) = await GetWithStatusAsync<PaginatedResponse<TenantItem>>(
            $"{GetIdentityUrl()}/api/v1/admin/tenants?page={CurrentPage}&pageSize={PageSize}");
        Tenants = data?.Items ?? [];
        TotalCount = data?.TotalCount ?? 0;
        if (!ok) { Tenants = []; }
    }
}
