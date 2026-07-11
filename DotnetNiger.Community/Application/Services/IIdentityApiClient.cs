using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>
/// Client HTTP vers l'API Identity pour la gestion des utilisateurs du tenant.
/// L'authentification se fait via X-Api-Key (InternalApiKey) qui donne les claims Admin+SuperAdmin.
/// </summary>
public interface IIdentityApiClient
{
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserAsync(Guid id);
    Task<bool> UpdateUserStatusAsync(Guid id, bool isActive);
    Task<string?> RegisterUserAsync(string email, string password, string fullName, string? role = null);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> AssignRoleToUserAsync(Guid userId, string roleName);
    /// <summary>Retire un rôle spécifique à un utilisateur.</summary>
    Task<bool> RemoveUserRoleAsync(Guid userId, string roleName);
    /// <summary>Remplace tous les rôles d'un utilisateur par un seul (supprime les anciens, ajoute le nouveau).</summary>
    Task<bool> ReplaceUserRolesAsync(Guid userId, string newRole);

    /// <summary>Met à jour le profil d'un utilisateur (prénom, nom, avatar).</summary>
    Task<bool> UpdateUserProfileAsync(Guid id, string? firstName, string? lastName, string? avatarUrl);
}
