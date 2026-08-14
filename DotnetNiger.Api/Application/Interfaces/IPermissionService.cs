using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de gestion des permissions.</summary>
public interface IPermissionService
{
    /// <summary>Crée une nouvelle permission.</summary>
    Task<PermissionResponse> CreateAsync(CreatePermissionRequest request, CancellationToken ct = default);
    /// <summary>Récupère la liste paginée des permissions.</summary>
    Task<PaginatedResponse<PermissionResponse>> GetAllAsync(PaginationQuery pagination, CancellationToken ct = default);
    /// <summary>Récupère les permissions groupées par catégorie.</summary>
    Task<List<PermissionGroupResponse>> GetGroupedAsync(int page = 1, int pageSize = 200, CancellationToken ct = default);
    /// <summary>Récupère une permission par identifiant.</summary>
    Task<PermissionResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Supprime une permission.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    /// <summary>Assigne des permissions à un rôle.</summary>
    Task AssignToRoleAsync(Guid roleId, List<Guid> permissionIds, CancellationToken ct = default);
    /// <summary>Récupère les permissions d'un utilisateur.</summary>
    Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);
}
