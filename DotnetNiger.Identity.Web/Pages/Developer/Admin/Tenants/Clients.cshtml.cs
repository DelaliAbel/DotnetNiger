using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin;

[Authorize(Roles = "Admin,SuperAdmin")]
public class ClientsModel : BasePageModel
{
    public ClientsModel(IHttpClientFactory http, IConfiguration config)
        : base(http, config) { }

    [BindProperty(SupportsGet = true)]
    public Guid TenantId { get; set; }

    public List<ClientItem> Clients { get; set; } = [];

    [BindProperty]
    public CreateClientInput Input { get; set; } = new();

    [BindProperty]
    public EditClientInput EditInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadClientsAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var client = await GetAuthenticatedClientAsync();
        try
        {
            var body = JsonSerializer.Serialize(new
            {
                clientName = Input.ClientName,
                description = Input.Description,
                allowedGrantTypes = Input.AllowedGrantTypes?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? new[] { "authorization_code" },
                redirectUris = new[] { Input.RedirectUri }.Where(u => !string.IsNullOrEmpty(u)).ToArray(),
                postLogoutRedirectUris = new[] { Input.PostLogoutRedirectUri }.Where(u => !string.IsNullOrEmpty(u)).ToArray()
            });
            var resp = await client.PostAsync($"{GetIdentityUrl()}/api/v1/admin/tenants/{TenantId}/clients",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (resp.IsSuccessStatusCode)
            {
                SetMessage("Client créé avec succès.");
                Input = new();
            }
            else
            {
                var err = await resp.Content.ReadAsStringAsync();
                SetMessage($"Erreur : {err}", true);
            }
        }
        catch (Exception ex)
        {
            SetMessage($"Erreur : {ex.Message}", true);
        }

        await LoadClientsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid clientId)
    {
        var deleted = await DeleteAsync($"{GetIdentityUrl()}/api/v1/admin/tenants/{TenantId}/clients/{clientId}");
        if (deleted) SetMessage("Client supprimé avec succès.");
        await LoadClientsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(Guid clientId)
    {
        var client = await GetAuthenticatedClientAsync();
        var body = JsonSerializer.Serialize(new
        {
            clientName = EditInput.ClientName,
            description = EditInput.Description,
            allowedGrantTypes = EditInput.AllowedGrantTypes?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? new[] { "authorization_code" },
            redirectUris = new[] { EditInput.RedirectUri }.Where(u => !string.IsNullOrEmpty(u)).ToArray(),
            postLogoutRedirectUris = new[] { EditInput.PostLogoutRedirectUri }.Where(u => !string.IsNullOrEmpty(u)).ToArray()
        });

        try
        {
            var resp = await client.PutAsync($"{GetIdentityUrl()}/api/v1/admin/tenants/{TenantId}/clients/{clientId}",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (resp.IsSuccessStatusCode)
            {
                SetMessage("Client modifié avec succès.");
            }
            else
            {
                var err = await resp.Content.ReadAsStringAsync();
                SetMessage($"Erreur : {err}", true);
            }
        }
        catch (Exception ex)
        {
            SetMessage($"Erreur : {ex.Message}", true);
        }

        await LoadClientsAsync();
        return Page();
    }

    private async Task LoadClientsAsync()
    {
        var data = await GetWithStatusAsync<PaginatedResponse<ClientItem>>(
            $"{GetIdentityUrl()}/api/v1/admin/tenants/{TenantId}/clients?page={CurrentPage}&pageSize={PageSize}");
        Clients = data.Data?.Items ?? [];
        TotalCount = data.Data?.TotalCount ?? 0;
    }
}

public class ClientItem
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string? Description { get; set; }
    public List<string> AllowedGrantTypes { get; set; } = [];
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateClientInput
{
    public string ClientName { get; set; } = "";
    public string? Description { get; set; }
    public string? AllowedGrantTypes { get; set; }
    public string? RedirectUri { get; set; }
    public string? PostLogoutRedirectUri { get; set; }
}

public class EditClientInput
{
    public string? ClientName { get; set; }
    public string? Description { get; set; }
    public string? AllowedGrantTypes { get; set; }
    public string? RedirectUri { get; set; }
    public string? PostLogoutRedirectUri { get; set; }
}
