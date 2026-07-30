using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de gestion des rôles.</summary>
public interface IRoleService
{
    /// <summary>Crée un nouveau rôle.</summary>
    Task<RoleResponse> CreateAsync(CreateRoleRequest request);
    /// <summary>Récupère la liste paginée des rôles.</summary>
    Task<PaginatedResponse<RoleResponse>> GetAllAsync(PaginationQuery pagination);
    /// <summary>Met à jour un rôle existant.</summary>
    Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request);
    /// <summary>Supprime un rôle.</summary>
    Task DeleteAsync(Guid id);
    /// <summary>Récupère un rôle par identifiant.</summary>
    Task<RoleResponse?> GetByIdAsync(Guid id);
    /// <summary>Assigne un rôle à un utilisateur.</summary>
    Task AssignToUserAsync(Guid userId, Guid roleId);
    /// <summary>Retire un rôle à un utilisateur.</summary>
    Task RemoveFromUserAsync(Guid userId, Guid roleId);
    /// <summary>Récupère les rôles d'un utilisateur.</summary>
    Task<List<RoleResponse>> GetUserRolesAsync(Guid userId);
}
