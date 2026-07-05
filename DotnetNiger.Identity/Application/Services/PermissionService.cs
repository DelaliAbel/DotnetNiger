using Microsoft.EntityFrameworkCore;
using DotnetNiger.Common.DTOs.Requests;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IdentityDbContext _db;

    public PermissionService(IdentityDbContext db) => _db = db;

    public async Task<PermissionResponse> CreateAsync(CreatePermissionRequest request)
    {
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Category = request.Category,
            TenantId = request.TenantId
        };
        _db.Permissions.Add(permission);
        await _db.SaveChangesAsync();
        return MapToResponse(permission);
    }

    public async Task<PaginatedResponse<PermissionResponse>> GetByTenantAsync(Guid tenantId, PaginationQuery pagination)
    {
        var query = _db.Permissions.AsNoTracking().Where(p => p.TenantId == tenantId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Category).ThenBy(p => p.Name)
            .Skip((pagination.EnsurePage - 1) * pagination.EnsurePageSize)
            .Take(pagination.EnsurePageSize)
            .Select(p => new PermissionResponse(p.Id, p.Name, p.Category, p.TenantId))
            .ToListAsync();

        return new PaginatedResponse<PermissionResponse>(items, totalCount, pagination.EnsurePage, pagination.EnsurePageSize);
    }

    public async Task<List<PermissionGroupResponse>> GetGroupedByTenantAsync(Guid tenantId, int page = 1, int pageSize = 200)
    {
        var query = _db.Permissions.AsNoTracking()
            .Where(p => p.TenantId == tenantId);

        var totalCount = await query.CountAsync();

        var permissions = await query
            .OrderBy(p => p.Category).ThenBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PermissionResponse(p.Id, p.Name, p.Category, p.TenantId))
            .ToListAsync();

        return permissions
            .GroupBy(p => p.Category)
            .Select(g => new PermissionGroupResponse(g.Key, g.ToList()))
            .ToList();
    }

    public async Task<PermissionResponse?> GetByIdAsync(Guid id)
    {
        var permission = await _db.Permissions.FindAsync(id);
        return permission == null ? null : MapToResponse(permission);
    }

    public async Task DeleteAsync(Guid id)
    {
        var permission = await _db.Permissions.FindAsync(id);
        if (permission != null)
        {
            _db.Permissions.Remove(permission);
            await _db.SaveChangesAsync();
        }
    }

    public async Task AssignToRoleAsync(Guid roleId, List<Guid> permissionIds)
    {
        var role = await _db.Roles.FindAsync(roleId);
        if (role == null) throw new KeyNotFoundException("Rôle non trouvé");

        var existing = await _db.Set<Dictionary<string, object>>("RolePermission")
            .Where(rp => (Guid)rp["RoleId"] == roleId)
            .ToListAsync();
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
        await _db.SaveChangesAsync();
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var permissionIds = await _db.Set<Dictionary<string, object>>("RolePermission")
            .Where(rp => roleIds.Contains((Guid)rp["RoleId"]))
            .Select(rp => (Guid)rp["PermissionId"])
            .ToListAsync();

        return await _db.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Name)
            .ToListAsync();
    }

    private static PermissionResponse MapToResponse(Permission p) =>
        new(p.Id, p.Name, p.Category, p.TenantId);
}
