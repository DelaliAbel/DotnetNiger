using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Opérations d'administration : tableau de bord et gestion des utilisateurs.</summary>
public interface IAdminService
{
    /// <summary>Statistiques globales de la plateforme (contenus, membres, etc.).</summary>
    Task<DashboardResponse> GetDashboardAsync();
    /// <summary>Liste de tous les utilisateurs avec leurs profils membres enrichis.</summary>
    Task<List<UserResponse>> GetUsersAsync();
    /// <summary>Détail d'un utilisateur par son identifiant.</summary>
    Task<UserResponse?> GetUserAsync(Guid id);
    /// <summary>Active ou désactive un compte utilisateur.</summary>
    Task<bool> UpdateUserStatusAsync(Guid id, bool isActive);
    /// <summary>Définit le rôle d'équipe (staff) d'un membre.</summary>
    Task<bool> UpdateUserTeamAsync(Guid id, bool isTeamMember, string position);
    /// <summary>Crée un utilisateur via Identity et son profil membre associé.</summary>
    Task<UserResponse?> CreateUserAsync(CreateAdminUserRequest request);
    /// <summary>Supprime un utilisateur (profil membre + compte Identity).</summary>
    Task<bool> DeleteUserAsync(Guid id);
    /// <summary>Assigne un rôle à un utilisateur.</summary>
    Task<bool> AssignRoleToUserAsync(Guid userId, string roleName);
    /// <summary>Retire un rôle à un utilisateur.</summary>
    Task<bool> RemoveUserRoleAsync(Guid userId, string roleName);
    /// <summary>Remplace tous les rôles d'un utilisateur par un seul rôle (supprime les anciens).</summary>
    Task<bool> ReplaceUserRolesAsync(Guid userId, string newRole);
}
