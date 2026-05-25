using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetNiger.Identity.Web.Pages.Developer;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public ProfileModel(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    [BindProperty]
    public ProfileInput Input { get; set; } = new();

    public string Email { get; set; } = "";
    public string TenantId { get; set; } = "";
    public List<string> Roles { get; set; } = [];
    public string Message { get; set; } = "";
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        await LoadProfileAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var identityUrl = _config["Identity:BaseUrl"]?.TrimEnd('/');
        var client = _http.CreateClient();
        var token = await HttpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = JsonSerializer.Serialize(new
        {
            firstName = Input.FirstName,
            lastName = Input.LastName,
            avatarUrl = Input.AvatarUrl
        });

        var response = await client.PutAsync(
            $"{identityUrl}/api/v1/profile",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            Message = "Profil mis à jour avec succès.";
            IsError = false;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Message = $"Erreur : {error}";
            IsError = true;
        }

        await LoadProfileAsync();
        return Page();
    }

    private async Task LoadProfileAsync()
    {
        var identityUrl = _config["Identity:BaseUrl"]?.TrimEnd('/');
        var client = _http.CreateClient();
        var token = await HttpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var response = await client.GetAsync($"{identityUrl}/api/v1/profile");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var profile = JsonSerializer.Deserialize<ProfileResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (profile != null)
                {
                    Input.FirstName = profile.FirstName ?? "";
                    Input.LastName = profile.LastName ?? "";
                    Input.AvatarUrl = profile.AvatarUrl ?? "";
                    Email = profile.Email;
                    TenantId = profile.TenantId?.ToString() ?? "";
                    Roles = profile.Roles?.ToList() ?? [];
                }
            }
        }
        catch
        {
            // fallback to claims
            Input.FirstName = User.FindFirst("given_name")?.Value ?? User.FindFirst(ClaimTypes.GivenName)?.Value ?? "";
            Input.LastName = User.FindFirst("family_name")?.Value ?? User.FindFirst(ClaimTypes.Surname)?.Value ?? "";
            Email = User.FindFirst("email")?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            TenantId = User.FindFirst("tenant_id")?.Value ?? "";
            Roles = User.FindAll("role").Select(c => c.Value).ToList();
        }
    }

    [BindProperty]
    public ChangePasswordInput PasswordInput { get; set; } = new();

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var identityUrl = _config["Identity:BaseUrl"]?.TrimEnd('/');
        var client = _http.CreateClient();
        var token = await HttpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = JsonSerializer.Serialize(new
        {
            currentPassword = PasswordInput.CurrentPassword,
            newPassword = PasswordInput.NewPassword
        });

        var response = await client.PostAsync(
            $"{identityUrl}/api/account/change-password",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            Message = "Mot de passe changé avec succès.";
            IsError = false;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Message = $"Erreur : {error}";
            IsError = true;
        }

        return Page();
    }

    public class ChangePasswordInput
    {
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}

public class ProfileInput
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
}

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? TenantId { get; set; }
    public List<string>? Roles { get; set; }
}
