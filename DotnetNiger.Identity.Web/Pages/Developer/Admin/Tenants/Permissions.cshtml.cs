using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetNiger.Identity.Web.Pages.Developer.Admin.Tenants;

[Authorize(Roles = "Admin")]
public class PermissionsModel : PageModel
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public PermissionsModel(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    [BindProperty(SupportsGet = true)]
    public Guid TenantId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 50;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

    public List<PermissionGroupItem> GroupedPermissions { get; set; } = [];
    public List<PermissionItem> AllPermissions { get; set; } = [];
    public List<RoleItem> Roles { get; set; } = [];
    public string Message { get; set; } = "";
    public bool IsError { get; set; }

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

        var identityUrl = _config["Identity:BaseUrl"]?.TrimEnd('/');
        var client = await CreateClientAsync(identityUrl);

        var body = JsonSerializer.Serialize(new
        {
            name = CreateInput.Name,
            category = CreateInput.Category,
            tenantId = TenantId
        });

        var response = await client.PostAsync(
            $"{identityUrl}/api/v1/{TenantId}/permissions",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            Message = "Permission créée.";
            IsError = false;
            CreateInput = new CreatePermissionInput();
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Message = $"Erreur : {error}";
            IsError = true;
        }

        await LoadAllAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid permissionId)
    {
        var identityUrl = _config["Identity:BaseUrl"]?.TrimEnd('/');
        var client = await CreateClientAsync(identityUrl);

        var response = await client.DeleteAsync($"{identityUrl}/api/v1/{TenantId}/permissions/{permissionId}");

        if (response.IsSuccessStatusCode)
        {
            Message = "Permission supprimée.";
            IsError = false;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Message = $"Erreur : {error}";
            IsError = true;
        }

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

        var identityUrl = _config["Identity:BaseUrl"]?.TrimEnd('/');
        var client = await CreateClientAsync(identityUrl);

        var selectedIds = Request.Form["AssignInput.PermissionIds"]
            .SelectMany(v => (v ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();

        var body = JsonSerializer.Serialize(new
        {
            roleId = AssignInput.RoleId,
            permissionIds = selectedIds
        });

        var response = await client.PostAsync(
            $"{identityUrl}/api/v1/{TenantId}/permissions/assign",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            Message = $"Permissions assignées au rôle.";
            IsError = false;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Message = $"Erreur : {error}";
            IsError = true;
        }

        await LoadAllAsync();
        return Page();
    }

    private async Task LoadAllAsync()
    {
        var identityUrl = _config["Identity:BaseUrl"]?.TrimEnd('/');
        var client = await CreateClientAsync(identityUrl);

        var tasks = new List<Task>
        {
            LoadPermissionsAsync(client, identityUrl),
            LoadRolesAsync(client, identityUrl)
        };

        await Task.WhenAll(tasks);
    }

    private async Task LoadPermissionsAsync(HttpClient client, string? identityUrl)
    {
        try
        {
            var resp = await client.GetAsync($"{identityUrl}/api/v1/{TenantId}/permissions/grouped");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var grouped = JsonSerializer.Deserialize<List<PermissionGroupItem>>(json, options);

                if (grouped != null)
                {
                    GroupedPermissions = grouped;
                    AllPermissions = grouped.SelectMany(g => g.Permissions).ToList();
                    TotalCount = AllPermissions.Count;
                }
            }
        }
        catch { }
    }

    private async Task LoadRolesAsync(HttpClient client, string? identityUrl)
    {
        try
        {
            var resp = await client.GetAsync($"{identityUrl}/api/v1/{TenantId}/roles?pageSize=100");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var paginated = JsonSerializer.Deserialize<RoleListResponse>(json, options);
                Roles = paginated?.Items ?? [];
            }
        }
        catch { }
    }

    private async Task<HttpClient> CreateClientAsync(string? identityUrl)
    {
        _ = identityUrl ?? throw new InvalidOperationException("Identity:BaseUrl n'est pas configuré.");
        var client = _http.CreateClient();
        var token = await HttpContext.GetTokenAsync("access_token");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
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
