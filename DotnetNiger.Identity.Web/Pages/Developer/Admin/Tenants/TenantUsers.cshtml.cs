using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin;

[Authorize(Roles = "Admin,SuperAdmin")]
public class TenantUsersModel : BasePageModel
{
    public TenantUsersModel(IHttpClientFactory http, IConfiguration config)
        : base(http, config) { }

    [BindProperty(SupportsGet = true)]
    public Guid TenantId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public List<UserItem> Users { get; set; } = [];

    [BindProperty]
    public CreateUserInput CreateInput { get; set; } = new();

    [BindProperty]
    public EditUserInput EditInput { get; set; } = new();

    [BindProperty]
    public ChangePasswordInput PasswordInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadUsersAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadUsersAsync();
            return Page();
        }

        var client = await GetAuthenticatedClientAsync();
        var body = JsonSerializer.Serialize(new
        {
            email = CreateInput.Email,
            password = CreateInput.Password,
            firstName = CreateInput.FirstName ?? "",
            lastName = CreateInput.LastName ?? "",
            tenantId = TenantId,
            roles = CreateInput.Role ? new[] { "User" } : Array.Empty<string>()
        });

        var response = await client.PostAsync(
            $"{GetIdentityUrl()}/api/v1/{TenantId}/users",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            SetMessage("Utilisateur créé.");
            CreateInput = new CreateUserInput();
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            SetMessage($"Erreur : {error}", true);
        }

        await LoadUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(Guid userId)
    {
        if (!ModelState.IsValid)
        {
            await LoadUsersAsync();
            return Page();
        }

        var ok = await PutAsync($"{GetIdentityUrl()}/api/v1/{TenantId}/users/{userId}", new
        {
            firstName = EditInput.FirstName ?? "",
            lastName = EditInput.LastName ?? ""
        });

        if (ok) SetMessage("Utilisateur mis à jour.");
        await LoadUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync(Guid userId)
    {
        var result = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/{TenantId}/users/{userId}/change-password", new
        {
            currentPassword = PasswordInput.CurrentPassword,
            newPassword = PasswordInput.NewPassword
        });

        if (result.Success)
        {
            SetMessage("Mot de passe changé.");
            PasswordInput = new ChangePasswordInput();
        }

        await LoadUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(Guid userId, bool isActive)
    {
        var ok = await PutAsync($"{GetIdentityUrl()}/api/v1/{TenantId}/users/{userId}", new { isActive = !isActive });
        if (ok) SetMessage(isActive ? "Utilisateur désactivé." : "Utilisateur activé.");
        await LoadUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid userId)
    {
        var deleted = await DeleteAsync($"{GetIdentityUrl()}/api/v1/{TenantId}/users/{userId}");
        if (deleted) SetMessage("Utilisateur supprimé.");
        await LoadUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostForgotPasswordAsync(Guid userId)
    {
        var user = Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            SetMessage("Utilisateur introuvable.", true);
            await LoadUsersAsync();
            return Page();
        }

        var client = await GetAuthenticatedClientAsync();
        var body = JsonSerializer.Serialize(new { email = user.Email });

        try
        {
            var response = await client.PostAsync(
                $"{GetIdentityUrl()}/api/v1/{TenantId}/users/forgot-password",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                SetMessage("Email de réinitialisation envoyé.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                SetMessage($"Erreur : {error}", true);
            }
        }
        catch (Exception ex)
        {
            SetMessage($"Erreur : {ex.Message}", true);
        }

        await LoadUsersAsync();
        return Page();
    }

    private async Task LoadUsersAsync()
    {
        var client = await GetAuthenticatedClientAsync();
        try
        {
            var resp = await client.GetAsync($"{GetIdentityUrl()}/api/v1/{TenantId}/users?page={CurrentPage}&pageSize={PageSize}");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var paginated = JsonSerializer.Deserialize<PaginatedResponse<UserItem>>(json, JsonOpts);
                Users = paginated?.Items ?? [];
                TotalCount = paginated?.TotalCount ?? 0;
            }
        }
        catch (Exception ex) { SetMessage($"Erreur lors du chargement : {ex.Message}", true); }
    }
}

public class UserItem
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class CreateUserInput
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool Role { get; set; } = true;
}


