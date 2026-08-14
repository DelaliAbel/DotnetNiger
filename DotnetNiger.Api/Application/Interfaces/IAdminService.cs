using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service d'administration des utilisateurs.</summary>
public interface IAdminService
{
    /// <summary>Envoie une invitation par email avec un rôle.</summary>
    Task InviteAsync(string email, string role, CancellationToken ct = default);
    /// <summary>Récupère tous les utilisateurs avec leurs rôles.</summary>
    Task<List<UserResponse>> GetAllUsersAsync(CancellationToken ct = default);
    /// <summary>Met à jour le statut actif/inactif d'un utilisateur.</summary>
    Task<bool> UpdateUserStatusAsync(Guid id, bool isActive, CancellationToken ct = default);
    /// <summary>Met à jour le statut équipe d'un utilisateur.</summary>
    Task<bool> UpdateUserTeamAsync(Guid id, bool isTeamMember, string position, CancellationToken ct = default);
    /// <summary>Assigne un rôle à un utilisateur.</summary>
    Task<bool> AssignRoleToUserAsync(Guid userId, string roleName, CancellationToken ct = default);
    /// <summary>Retire un rôle à un utilisateur.</summary>
    Task<bool> RemoveUserRoleAsync(Guid userId, string roleName, CancellationToken ct = default);
    /// <summary>Supprime un utilisateur.</summary>
    Task<bool> DeleteUserAsync(Guid id, Guid? callerId = null, CancellationToken ct = default);
    /// <summary>Récupère un utilisateur par identifiant avec ses rôles.</summary>
    Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Met à jour le profil d'un utilisateur.</summary>
    Task<UserResponse?> UpdateUserProfileAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    /// <summary>Crée un utilisateur avec un rôle spécifique.</summary>
    Task<UserResponse?> CreateUserAsync(AdminCreateUserRequest request, CancellationToken ct = default);
    /// <summary>Récupère les statistiques du tableau de bord.</summary>
    Task<DashboardStats> GetDashboardAsync(CancellationToken ct = default);
    /// <summary>Récupère les statistiques personnelles d'un collaborateur.</summary>
    Task<DashboardStats> GetCollaboratorDashboardAsync(Guid userId, CancellationToken ct = default);
}
