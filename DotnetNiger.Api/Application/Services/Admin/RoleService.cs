using System.Threading;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Admin;

/// <summary>Service de gestion des rôles et de leur assignation aux utilisateurs.</summary>
public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DotnetNigerDbContext _db;

    public RoleService(RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager, DotnetNigerDbContext db)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
    }

    /// <summary>Crée un nouveau rôle.</summary>
    public async Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var role = new ApplicationRole
        {
            Name = request.Name, Description = request.Description
        };
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        return new RoleResponse(role.Id, role.Name!, role.Description, 0);
    }

    /// <summary>Récupère la liste paginée des rôles avec le nombre d'utilisateurs.</summary>
    public async Task<PaginatedResponse<RoleResponse>> GetAllAsync(PaginationQuery pagination, CancellationToken ct = default)
    {
        var query = _db.Roles.AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var roles = await query
            .OrderBy(r => r.Id)
            .Skip((pagination.EnsurePage - 1) * pagination.EnsurePageSize)
            .Take(pagination.EnsurePageSize)
            .ToListAsync(ct);

        var roleIds = roles.Select(r => r.Id).ToList();
        var userCounts = await _db.UserRoles
            .Where(ur => roleIds.Contains(ur.RoleId))
            .GroupBy(ur => ur.RoleId)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var countsMap = userCounts.ToDictionary(x => x.RoleId, x => x.Count);

        var items = roles.Select(r => new RoleResponse(
            r.Id, r.Name!, r.Description,
            countsMap.GetValueOrDefault(r.Id, 0))).ToList();

        return new PaginatedResponse<RoleResponse>(items, totalCount, pagination.EnsurePage, pagination.EnsurePageSize);
    }

    /// <summary>Met à jour la description d'un rôle.</summary>
    public async Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) throw new KeyNotFoundException("Rôle non trouvé");

        role.Description = request.Description ?? role.Description;
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var count = await _db.UserRoles.CountAsync(ur => ur.RoleId == role.Id, ct);
        return new RoleResponse(role.Id, role.Name!, role.Description, count);
    }

    /// <summary>Supprime un rôle par son identifiant.</summary>
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role != null) await _roleManager.DeleteAsync(role);
    }

    /// <summary>Récupère un rôle par son identifiant avec le nombre d'utilisateurs.</summary>
    public async Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return null;
        var count = await _db.UserRoles.CountAsync(ur => ur.RoleId == role.Id, ct);
        return new RoleResponse(role.Id, role.Name!, role.Description, count);
    }

    /// <summary>Assigne un rôle à un utilisateur en remplaçant les rôles existants.</summary>
    public async Task AssignToUserAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (user == null || role == null) throw new KeyNotFoundException();

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count != 0)
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role.Name!);
    }

    /// <summary>Retire un rôle à un utilisateur.</summary>
    public async Task RemoveFromUserAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (user == null || role == null) throw new KeyNotFoundException();

        await _userManager.RemoveFromRoleAsync(user, role.Name!);
    }

    /// <summary>Récupère les rôles assignés à un utilisateur.</summary>
    public async Task<List<RoleResponse>> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new KeyNotFoundException();

        var roleNames = await _userManager.GetRolesAsync(user);
        var roles = await _db.Roles.AsNoTracking()
            .Where(r => roleNames.Contains(r.Name!))
            .ToListAsync(ct);

        return roles.Select(r => new RoleResponse(
            r.Id, r.Name!, r.Description, 0)).ToList();
    }
}
