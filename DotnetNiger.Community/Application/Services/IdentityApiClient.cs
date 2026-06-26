using System.Net.Http.Json;
using System.Text.Json;
using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Client HTTP vers l'API Identity distante pour la gestion des utilisateurs.</summary>
public class IdentityApiClient(HttpClient http) : IIdentityApiClient
{
    /// <summary>Récupère la liste de tous les utilisateurs depuis l'API distante.</summary>
    public async Task<List<UserDto>> GetUsersAsync()
    {
        var response = await http.GetAsync("api/v1/admin/users");
        if (!response.IsSuccessStatusCode) return [];
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        return users ?? [];
    }

    /// <summary>Détail d'un utilisateur distant.</summary>
    public async Task<UserDto?> GetUserAsync(Guid id)
    {
        var response = await http.GetAsync($"api/v1/admin/users/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    /// <summary>Active ou désactive un compte distant.</summary>
    public async Task<bool> UpdateUserStatusAsync(Guid id, bool isActive)
    {
        var response = await http.PatchAsJsonAsync($"api/v1/admin/users/{id}/status", new { isActive });
        return response.IsSuccessStatusCode;
    }

    /// <summary>Crée un utilisateur distant et retourne son identifiant.</summary>
    public async Task<string?> RegisterUserAsync(string email, string password, string fullName)
    {
        var parts = fullName.Split(' ', 2, StringSplitOptions.TrimEntries);
        var firstName = parts[0];
        var lastName = parts.Length > 1 ? parts[1] : ".";

        var response = await http.PostAsJsonAsync("api/v1/admin/users", new
        {
            email,
            password,
            firstName,
            lastName,
            role = ""
        });
        if (!response.IsSuccessStatusCode) return null;
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        if (doc?.RootElement.TryGetProperty("id", out var idProp) == true)
            return idProp.GetString();
        return null;
    }

    /// <summary>Supprime un utilisateur distant.</summary>
    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var response = await http.DeleteAsync($"api/v1/admin/users/{id}");
        return response.IsSuccessStatusCode;
    }

    /// <summary>Assigne un rôle à un utilisateur distant.</summary>
    public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName)
    {
        var response = await http.PostAsJsonAsync($"api/v1/admin/users/{userId}/roles", new { roleName });
        return response.IsSuccessStatusCode;
    }

    /// <summary>Remplace tous les rôles d'un utilisateur par un seul (supprime les anciens via DELETE puis ajoute le nouveau).</summary>
    public async Task<bool> ReplaceUserRolesAsync(Guid userId, string newRole)
    {
        var user = await GetUserAsync(userId);
        if (user is null) return false;

        foreach (var role in user.Roles)
        {
            if (!role.Equals(newRole, StringComparison.OrdinalIgnoreCase))
            {
                await http.DeleteAsync($"api/v1/admin/users/{userId}/roles/{role}");
            }
        }

        return await AssignRoleToUserAsync(userId, newRole);
    }
}
