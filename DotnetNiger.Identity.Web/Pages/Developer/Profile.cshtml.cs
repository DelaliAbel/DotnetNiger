using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Identity.Web.Infrastructure;
using DotnetNiger.Identity.Web.Models;

namespace DotnetNiger.Identity.Web.Pages.Developer;

[Authorize]
public class ProfileModel : BasePageModel
{
    public ProfileModel(IHttpClientFactory http, IConfiguration config, ILogger<ProfileModel> logger)
        : base(http, config, logger) { }

    [BindProperty]
    public ProfileInput Input { get; set; } = new();

    public string Email { get; set; } = "";
    public string TenantId { get; set; } = "";
    public List<string> Roles { get; set; } = [];

    public bool TwoFactorEnabled { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public string? SharedKey { get; set; }
    public string? AuthenticatorUri { get; set; }
    public string[]? RecoveryCodes { get; set; }

    [BindProperty]
    public TwoFactorInput TwoFactorInput { get; set; } = new();

    [BindProperty]
    public ChangeEmailInput ChangeEmailInput { get; set; } = new();

    [BindProperty]
    public ConfirmChangeEmailInput ConfirmChangeEmailInput { get; set; } = new();

    [BindProperty]
    public ChangePasswordInput PasswordInput { get; set; } = new();

    public List<LoginHistoryEntry> LoginHistory { get; set; } = [];

    public async Task OnGetAsync()
    {
        await LoadProfileAsync();
        await LoadTwoFactorStatusAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var ok = await PutAsync($"{GetIdentityUrl()}/api/v1/profile", new
        {
            firstName = Input.FirstName,
            lastName = Input.LastName,
            avatarUrl = Input.AvatarUrl
        });
        if (!ok) return Page();

        SetMessage("Profil mis à jour avec succès.");
        await LoadProfileAsync();
        await LoadTwoFactorStatusAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        if (!ModelState.IsValid) return Page();

        var (_, success) = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/profile/change-password", new
        {
            currentPassword = PasswordInput.CurrentPassword,
            newPassword = PasswordInput.NewPassword
        });
        if (success) SetMessage("Mot de passe changé avec succès.");
        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        if (!ModelState.IsValid) return Page();

        var (_, success) = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/profile/change-email", new
        {
            newEmail = ChangeEmailInput.NewEmail
        });

        if (success)
        {
            SetMessage("Un code de confirmation a été envoyé à votre nouvelle adresse email.");
        }

        await LoadProfileAsync();
        await LoadTwoFactorStatusAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostConfirmChangeEmailAsync()
    {
        if (!ModelState.IsValid) return Page();

        var (_, success) = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/profile/confirm-change-email", new
        {
            code = ConfirmChangeEmailInput.Code
        });

        if (success)
        {
            SetMessage("Adresse email modifiée avec succès.");
            ConfirmChangeEmailInput = new ConfirmChangeEmailInput();
        }

        await LoadProfileAsync();
        await LoadTwoFactorStatusAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAccountAsync(string confirmEmail, string confirmText)
    {
        if (confirmEmail != Email)
        {
            SetMessage("L'email de confirmation ne correspond pas.", true);
            return Page();
        }

        if (confirmText != "SUPPRIMER")
        {
            SetMessage("Veuillez taper SUPPRIMER pour confirmer.", true);
            return Page();
        }

        var deleted = await DeleteAsync($"{GetIdentityUrl()}/api/v1/profile");
        if (deleted)
        {
            await HttpContext.SignOutAsync();
            return Redirect("/Account/Login?message=compte_supprime");
        }

        await LoadProfileAsync();
        await LoadTwoFactorStatusAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostLoadLoginHistoryAsync()
    {
        var paginated = await GetAsync<PaginatedResponse<LoginHistoryEntry>>(
            $"{GetIdentityUrl()}/api/v1/profile/login-history?pageSize=20");
        LoginHistory = paginated?.Items ?? [];

        await LoadProfileAsync();
        await LoadTwoFactorStatusAsync();
        return Page();
    }

    public async Task<IActionResult> OnGetTwoFactorStatusAsync()
    {
        await LoadTwoFactorStatusAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSetupTwoFactorAsync()
    {
        var (result, success) = await PostAsync<TwoFactorSetupResponse>($"{GetIdentityUrl()}/api/v1/profile/two-factor/setup");
        if (success && result != null)
        {
            SharedKey = result.SharedKey;
            AuthenticatorUri = result.AuthenticatorUri;
            SetMessage("Scannez le code QR avec votre application d'authentification.");
        }

        await LoadTwoFactorStatusAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostEnableTwoFactorAsync()
    {
        if (string.IsNullOrEmpty(TwoFactorInput.Code))
        {
            SetMessage("Veuillez entrer le code de vérification.", true);
            return Page();
        }

        var (result, success) = await PostAsync<Enable2FAResponse>($"{GetIdentityUrl()}/api/v1/profile/two-factor/enable", new
        {
            code = TwoFactorInput.Code
        });

        if (success)
        {
            RecoveryCodes = result?.RecoveryCodes;
            SetMessage("Double authentification activée avec succès.");
        }

        await LoadTwoFactorStatusAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDisableTwoFactorAsync()
    {
        if (string.IsNullOrEmpty(TwoFactorInput.Code))
        {
            SetMessage("Veuillez entrer le code de vérification.", true);
            return Page();
        }

        var (_, success) = await PostAsync<object>($"{GetIdentityUrl()}/api/v1/profile/two-factor/disable", new
        {
            code = TwoFactorInput.Code
        });

        if (success)
        {
            SharedKey = null;
            AuthenticatorUri = null;
            RecoveryCodes = null;
            SetMessage("Double authentification désactivée.");
        }

        await LoadTwoFactorStatusAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostGenerateRecoveryCodesAsync()
    {
        var (result, success) = await PostAsync<Enable2FAResponse>($"{GetIdentityUrl()}/api/v1/profile/two-factor/recovery-codes");
        if (success)
        {
            RecoveryCodes = result?.RecoveryCodes;
            SetMessage("Codes de récupération générés. Sauvegardez-les dans un endroit sûr.");
        }

        await LoadTwoFactorStatusAsync();
        return Page();
    }

    private async Task LoadProfileAsync()
    {
        var profile = await GetAsync<ProfileResponse>($"{GetIdentityUrl()}/api/v1/profile");
        if (profile != null)
        {
            Input.FirstName = profile.FirstName ?? "";
            Input.LastName = profile.LastName ?? "";
            Input.AvatarUrl = profile.AvatarUrl ?? "";
            Email = profile.Email;
            TenantId = profile.TenantId?.ToString() ?? "";
            Roles = profile.Roles?.ToList() ?? [];
        }
        else
        {
            Input.FirstName = User.FindFirst("given_name")?.Value ?? User.FindFirst(ClaimTypes.GivenName)?.Value ?? "";
            Input.LastName = User.FindFirst("family_name")?.Value ?? User.FindFirst(ClaimTypes.Surname)?.Value ?? "";
            Email = User.FindFirst("email")?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            TenantId = User.FindFirst("tenant_id")?.Value ?? "";
            Roles = User.FindAll("role").Select(c => c.Value).ToList();
        }
    }

    private async Task LoadTwoFactorStatusAsync()
    {
        var result = await GetAsync<TwoFactorStatusResponse>($"{GetIdentityUrl()}/api/v1/profile/two-factor/status");
        if (result != null)
        {
            TwoFactorEnabled = result.TwoFactorEnabled;
            RecoveryCodesLeft = result.RecoveryCodesLeft;
        }
    }
}

