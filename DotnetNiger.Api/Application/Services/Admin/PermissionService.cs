using System.Threading;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Admin;

/// <summary>Service de gestion des permissions et de leur assignation aux rôles.</summary>
public class PermissionService : IPermissionService
{
    private readonly DotnetNigerDbContext _db;

    public PermissionService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Crée une nouvelle permission.</summary>
    public async Task<PermissionResponse> CreateAsync(CreatePermissionRequest request, CancellationToken ct = default)
    {
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Category = request.Category
        };
        _db.Permissions.Add(permission);
        await _db.SaveChangesAsync(ct);
        return MapToResponse(permission);
    }

    /// <summary>Récupère la liste paginée des permissions.</summary>
    public async Task<PaginatedResponse<PermissionResponse>> GetAllAsync(PaginationQuery pagination, CancellationToken ct = default)
    {
        var query = _db.Permissions.AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Category).ThenBy(p => p.Name)
            .Skip((pagination.EnsurePage - 1) * pagination.EnsurePageSize)
            .Take(pagination.EnsurePageSize)
            .Select(p => new PermissionResponse(p.Id, p.Name, p.Category))
            .ToListAsync(ct);

        return new PaginatedResponse<PermissionResponse>(items, totalCount, pagination.EnsurePage, pagination.EnsurePageSize);
    }

    /// <summary>Récupère les permissions groupées par catégorie.</summary>
    public async Task<List<PermissionGroupResponse>> GetGroupedAsync(int page = 1, int pageSize = 200, CancellationToken ct = default)
    {
        var query = _db.Permissions.AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var permissions = await query
            .OrderBy(p => p.Category).ThenBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PermissionResponse(p.Id, p.Name, p.Category))
            .ToListAsync(ct);

        return permissions
            .GroupBy(p => p.Category)
            .Select(g => new PermissionGroupResponse(g.Key, g.ToList()))
            .ToList();
    }

    /// <summary>Récupère une permission par son identifiant.</summary>
    public async Task<PermissionResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var permission = await _db.Permissions.FindAsync(id, ct);
        return permission == null ? null : MapToResponse(permission);
    }

    /// <summary>Supprime une permission par son identifiant.</summary>
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var permission = await _db.Permissions.FindAsync(id, ct);
        if (permission != null)
        {
            _db.Permissions.Remove(permission);
            await _db.SaveChangesAsync(ct);
        }
    }

    /// <summary>Assigne des permissions à un rôle en remplaçant les existantes.</summary>
    public async Task AssignToRoleAsync(Guid roleId, List<Guid> permissionIds, CancellationToken ct = default)
    {
        var role = await _db.Roles.FindAsync(roleId, ct);
        if (role == null) throw new KeyNotFoundException("Rôle non trouvé");

        var existing = await _db.Set<Dictionary<string, object>>("RolePermission")
            .Where(rp => (Guid)rp["RoleId"] == roleId)
            .ToListAsync(ct);
        _db.Set<Dictionary<string, object>>("RolePermission").RemoveRange(existing);

        foreach (var permId in permissionIds)
        {
            _db.Set<Dictionary<string, object>>("RolePermission").Add(
                new Dictionary<string, object>
                {
                    ["RoleId"] = roleId,
                    ["PermissionId"] = permId
                });
        }
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Récupère les noms des permissions d'un utilisateur via ses rôles.</summary>
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(ct);

        var permissionIds = await _db.Set<Dictionary<string, object>>("RolePermission")
            .Where(rp => roleIds.Contains((Guid)rp["RoleId"]))
            .Select(rp => (Guid)rp["PermissionId"])
            .ToListAsync(ct);

        return await _db.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Name)
            .ToListAsync(ct);
    }

    private static PermissionResponse MapToResponse(Permission p) =>
        new(p.Id, p.Name, p.Category);
}
