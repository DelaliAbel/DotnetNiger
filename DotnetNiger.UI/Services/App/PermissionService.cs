using DotnetNiger.UI.Services.Contracts;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Services.Auth;
using System.Security.Claims;
using System.Threading;

namespace DotnetNiger.UI.Services.App;

/// <summary>
/// Service de permissions côté client.
/// Les permissions sont maintenant extraites du JWT (claim "permission")
/// au lieu d'appeler l'endpoint /api/auth/userinfo.
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly CustomAuthStateProvider _authStateProvider;
    private HashSet<string> _permissions = [];

    public PermissionService(CustomAuthStateProvider authStateProvider) => _authStateProvider = authStateProvider;

    public IReadOnlySet<string> Permissions => _permissions;

    public bool HasPermission(string permissionName) =>
        _permissions.Contains(permissionName);

    public async Task LoadPermissionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _authStateProvider.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                _permissions = [];
                return;
            }

            var claims = JwtParser.ParseClaimsFromJwt(token);
            _permissions = claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            _permissions = [];
        }
    }

    public void Clear() => _permissions = [];
}
