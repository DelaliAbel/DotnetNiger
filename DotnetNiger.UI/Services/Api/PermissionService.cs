using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Api;

public class PermissionService : IPermissionService
{
    private readonly HttpClient _http;
    private HashSet<string> _permissions = [];

    public PermissionService(HttpClient http) => _http = http;

    public IReadOnlySet<string> Permissions => _permissions;

    public bool HasPermission(string permissionName) =>
        _permissions.Contains(permissionName);

    public async Task LoadPermissionsAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/auth/userinfo");
            if (!response.IsSuccessStatusCode)
            {
                _permissions = [];
                return;
            }

            using var doc = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = doc.RootElement;

            if (root.TryGetProperty("permissions", out var perms) && perms.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                _permissions = perms.EnumerateArray()
                    .Select(p => p.GetString()!)
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToHashSet();
            }
            else
            {
                _permissions = [];
            }
        }
        catch
        {
            _permissions = [];
        }
    }

    public void Clear() => _permissions = [];
}
