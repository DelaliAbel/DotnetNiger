using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer;

[Authorize]
public class ApiKeysModel : BasePageModel
{
    public ApiKeysModel(IHttpClientFactory http, IConfiguration config)
        : base(http, config) { }

    public List<ApiKeyItem> ApiKeys { get; set; } = [];

    [BindProperty]
    public string NewKeyName { get; set; } = "";

    public async Task OnGetAsync()
    {
        await LoadKeysAsync();
    }

    private string GetTenantId() => User.FindFirst("tenant_id")?.Value ?? "";

    private string GetApiKeysBaseUrl()
    {
        var tenantId = GetTenantId();
        return $"{GetIdentityUrl()}/api/v1/admin/tenants/{tenantId}/api-keys";
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewKeyName))
        {
            SetMessage("Le nom de la clé est requis.", true);
            await LoadKeysAsync();
            return Page();
        }

        var (created, ok) = await PostAsync<ApiKeyCreatedResponse>(GetApiKeysBaseUrl(), new { name = NewKeyName });
        if (ok)
        {
            NewKeyName = "";
            if (created != null)
                SetMessage($"Clé créée : {created.Key}. Copiez-la maintenant, elle ne sera plus affichée.");
            else
                SetMessage("Clé créée avec succès.");
        }

        await LoadKeysAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid keyId)
    {
        var deleted = await DeleteAsync($"{GetApiKeysBaseUrl()}/{keyId}");
        if (deleted) SetMessage("Clé supprimée.");
        await LoadKeysAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRotateAsync(Guid keyId)
    {
        var (rotated, ok) = await PostAsync<ApiKeyCreatedResponse>($"{GetApiKeysBaseUrl()}/{keyId}/rotate");
        if (ok)
        {
            SetMessage(rotated != null
                ? $"Nouvelle clé : {rotated.Key}. Copiez-la maintenant."
                : "Clé rotée avec succès.");
        }

        await LoadKeysAsync();
        return Page();
    }

    private async Task LoadKeysAsync()
    {
        var paginated = await GetAsync<PaginatedResponse<ApiKeyItem>>($"{GetApiKeysBaseUrl()}?pageSize=100");
        ApiKeys = paginated?.Items ?? [];
    }
}
