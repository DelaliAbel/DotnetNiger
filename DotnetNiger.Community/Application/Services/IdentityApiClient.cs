using System.Net.Http.Json;
using System.Text.Json;
using DotnetNiger.Community.Application.DTOs.Responses;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Client HTTP vers l'API Identity distante avec cache local et dégradation progressive.</summary>
public class IdentityApiClient(HttpClient http, IMemoryCache cache, ILogger<IdentityApiClient> logger) : IIdentityApiClient
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
    private const string UsersCacheKey = "identity_users";

    /// <summary>Récupère la liste de tous les utilisateurs (avec cache 5 min).</summary>
    public async Task<List<UserResponse>> GetUsersAsync()
    {
        if (cache.TryGetValue(UsersCacheKey, out List<UserResponse>? cached))
            return cached!;

        try
        {
            var response = await http.GetAsync("api/v1/admin/users");
            if (!response.IsSuccessStatusCode)
                return GetCachedUsersOrEmpty();

            var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
            if (users is not null) cache.Set(UsersCacheKey, users, CacheTtl);
            return users ?? [];
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Identity API injoignable pour GetUsersAsync");
            return GetCachedUsersOrEmpty();
        }
    }

    /// <summary>Détail d'un utilisateur distant (avec cache 5 min).</summary>
    public async Task<UserResponse?> GetUserAsync(Guid id)
    {
        var key = $"identity_user_{id}";
        if (cache.TryGetValue(key, out UserResponse? cached))
            return cached;

        try
        {
            var response = await http.GetAsync($"api/v1/admin/users/{id}");
            if (!response.IsSuccessStatusCode)
                return GetCachedUserOrNull(key);

            var user = await response.Content.ReadFromJsonAsync<UserResponse>();
            if (user is not null) cache.Set(key, user, CacheTtl);
            return user;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Identity API injoignable pour GetUserAsync({Id})", id);
            return GetCachedUserOrNull(key);
        }
    }

    /// <summary>Active ou désactive un compte distant.</summary>
    public async Task<bool> UpdateUserStatusAsync(Guid id, bool isActive)
    {
        try
        {
            var response = await http.PatchAsJsonAsync($"api/v1/admin/users/{id}/status", new { isActive });
            InvalidateUserCache(id);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Identity API injoignable pour UpdateUserStatusAsync({Id})", id);
            return false;
        }
    }

    /// <summary>Crée un utilisateur distant et retourne son identifiant.</summary>
    public async Task<string?> RegisterUserAsync(string email, string password, string fullName, string? role = null)
    {
        var (firstName, lastName) = SplitFullName(fullName);

        try
        {
            var response = await http.PostAsJsonAsync("api/v1/admin/users", new
            {
                email, password, firstName, lastName, role = role ?? ""
            });
            if (!response.IsSuccessStatusCode) return null;

            InvalidateUsersCache();
            var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
            if (doc?.RootElement.TryGetProperty("id", out var idProp) == true)
                return idProp.GetString();
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Identity API injoignable pour RegisterUserAsync");
            return null;
        }
    }

    /// <summary>Supprime un utilisateur distant.</summary>
    public async Task<bool> DeleteUserAsync(Guid id)
    {
        try
        {
            var response = await http.DeleteAsync($"api/v1/admin/users/{id}");
            InvalidateUserCache(id);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Identity API injoignable pour DeleteUserAsync({Id})", id);
            return false;
        }
    }

    /// <summary>Retire un rôle à un utilisateur distant.</summary>
    public async Task<bool> RemoveUserRoleAsync(Guid userId, string roleName)
    {
        try
        {
            var response = await http.DeleteAsync($"api/v1/admin/users/{userId}/roles/{roleName}");
            InvalidateUserCache(userId);
            InvalidateUsersCache();
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Identity API injoignable pour RemoveUserRoleAsync({UserId})", userId);
            return false;
        }
    }

    /// <summary>Assigne un rôle à un utilisateur distant.</summary>
    public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName)
    {
        try
        {
            var response = await http.PostAsJsonAsync($"api/v1/admin/users/{userId}/roles", new { roleName });
            InvalidateUserCache(userId);
            InvalidateUsersCache();
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Identity API injoignable pour AssignRoleToUserAsync({UserId})", userId);
            return false;
        }
    }

    /// <summary>Remplace tous les rôles d'un utilisateur par un seul.</summary>
    public async Task<bool> ReplaceUserRolesAsync(Guid userId, string newRole)
    {
        try
        {
            var user = await GetUserAsync(userId);
            if (user is null) return false;

            foreach (var role in user.Roles)
            {
                if (!role.Equals(newRole, StringComparison.OrdinalIgnoreCase))
                    await http.DeleteAsync($"api/v1/admin/users/{userId}/roles/{role}");
            }

            var result = await AssignRoleToUserAsync(userId, newRole);
            InvalidateUserCache(userId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Identity API injoignable pour ReplaceUserRolesAsync({UserId})", userId);
            return false;
        }
    }

    /// <summary>Met à jour le profil d'un utilisateur distant.</summary>
    public async Task<bool> UpdateUserProfileAsync(Guid id, string? firstName, string? lastName, string? avatarUrl)
    {
        try
        {
            var response = await http.PatchAsJsonAsync($"api/v1/admin/users/{id}/profile", new
            {
                firstName, lastName, avatarUrl
            });
            InvalidateUserCache(id);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Identity API injoignable pour UpdateUserProfileAsync({Id})", id);
            return false;
        }
    }

    private void InvalidateUserCache(Guid id) => cache.Remove($"identity_user_{id}");
    private void InvalidateUsersCache() => cache.Remove(UsersCacheKey);

    private List<UserResponse> GetCachedUsersOrEmpty() =>
        cache.TryGetValue(UsersCacheKey, out List<UserResponse>? cached) ? cached! : [];

    private UserResponse? GetCachedUserOrNull(string key) =>
        cache.TryGetValue(key, out UserResponse? cached) ? cached : null;

    /// <summary>Sépare un nom complet en prénom et nom, gérant les noms composés.</summary>
    private static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return ("", ".");

        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.TrimEntries);
        var first = parts[0];
        var last = parts.Length > 1 ? parts[1] : ".";
        return (first, last);
    }
}
