using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin;

[Authorize(Roles = "Admin,SuperAdmin")]
public class TenantApiKeysModel : BasePageModel
{
    public TenantApiKeysModel(IHttpClientFactory http, IConfiguration config)
        : base(http, config) { }

    [BindProperty(SupportsGet = true)]
    public Guid TenantId { get; set; }

    public List<ApiKeyItem> ApiKeys { get; set; } = [];

    [BindProperty]
    public string NewKeyName { get; set; } = "";

    public async Task OnGetAsync()
    {
        await LoadKeysAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewKeyName))
        {
            SetMessage("Le nom de la clé est requis.", true);
            await LoadKeysAsync();
            return Page();
        }

        var result = await PostAsync<ApiKeyCreatedResponse>($"{GetIdentityUrl()}/api/v1/admin/tenants/{TenantId}/api-keys", new
        {
            name = NewKeyName
        });

        if (result.Success)
        {
            if (result.Data != null)
                SetMessage($"Clé créée : {result.Data.Key}. Copiez-la maintenant, elle ne sera plus affichée.");
            else
                SetMessage("Clé créée avec succès.");
            NewKeyName = "";
        }

        await LoadKeysAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid keyId)
    {
        var deleted = await DeleteAsync($"{GetIdentityUrl()}/api/v1/admin/tenants/{TenantId}/api-keys/{keyId}");
        if (deleted) SetMessage("Clé supprimée.");
        await LoadKeysAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRotateAsync(Guid keyId)
    {
        var result = await PostAsync<ApiKeyCreatedResponse>($"{GetIdentityUrl()}/api/v1/admin/tenants/{TenantId}/api-keys/{keyId}/rotate");

        if (result.Success)
        {
            SetMessage(result.Data != null
                ? $"Nouvelle clé : {result.Data.Key}. Copiez-la maintenant."
                : "Clé rotée avec succès.");
        }

        await LoadKeysAsync();
        return Page();
    }

    private async Task LoadKeysAsync()
    {
        var data = await GetWithStatusAsync<PaginatedResponse<ApiKeyItem>>(
            $"{GetIdentityUrl()}/api/v1/admin/tenants/{TenantId}/api-keys?page={CurrentPage}&pageSize={PageSize}");
        ApiKeys = data.Data?.Items ?? [];
        TotalCount = data.Data?.TotalCount ?? 0;
    }
}
