using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service d'administration des utilisateurs.</summary>
public interface IAdminService
{
    /// <summary>Envoie une invitation par email avec un rôle.</summary>
    Task InviteAsync(string email, string role);
    /// <summary>Récupère tous les utilisateurs avec leurs rôles.</summary>
    Task<List<UserResponse>> GetAllUsersAsync();
    /// <summary>Récupère la liste des utilisateurs.</summary>
    Task<List<UserResponse>> GetUsersAsync();
    /// <summary>Récupère un utilisateur par identifiant.</summary>
    Task<UserResponse?> GetUserAsync(Guid id);
    /// <summary>Met à jour le statut actif/inactif d'un utilisateur.</summary>
    Task<bool> UpdateUserStatusAsync(Guid id, bool isActive);
    /// <summary>Met à jour le statut équipe d'un utilisateur.</summary>
    Task<bool> UpdateUserTeamAsync(Guid id, bool isTeamMember, string position);
    /// <summary>Assigne un rôle à un utilisateur.</summary>
    Task<bool> AssignRoleToUserAsync(Guid userId, string roleName);
    /// <summary>Remplace les rôles d'un utilisateur.</summary>
    Task<bool> ReplaceUserRolesAsync(Guid userId, string roleName);
    /// <summary>Retire un rôle à un utilisateur.</summary>
    Task<bool> RemoveUserRoleAsync(Guid userId, string roleName);
    /// <summary>Supprime un utilisateur.</summary>
    Task<bool> DeleteUserAsync(Guid id, Guid? callerId = null);
    /// <summary>Récupère un utilisateur par identifiant avec ses rôles.</summary>
    Task<UserResponse?> GetUserByIdAsync(Guid id);
    /// <summary>Met à jour le profil d'un utilisateur.</summary>
    Task<UserResponse?> UpdateUserProfileAsync(Guid id, UpdateUserRequest request);
    /// <summary>Crée un utilisateur avec un rôle spécifique.</summary>
    Task<UserResponse?> CreateUserAsync(AdminCreateUserRequest request);
    /// <summary>Crée un utilisateur admin.</summary>
    Task<UserResponse?> CreateUserAsync(CreateAdminUserRequest request);
    /// <summary>Récupère les statistiques du tableau de bord.</summary>
    Task<DashboardStats> GetDashboardAsync();
    /// <summary>Récupère les statistiques personnelles d'un collaborateur.</summary>
    Task<DashboardStats> GetCollaboratorDashboardAsync(Guid userId);
}
