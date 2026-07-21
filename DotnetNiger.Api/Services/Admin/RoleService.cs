using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Admin;

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

    public async Task<RoleResponse> CreateAsync(CreateRoleRequest request)
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

    public async Task<PaginatedResponse<RoleResponse>> GetAllAsync(PaginationQuery pagination)
    {
        var query = _db.Roles.AsNoTracking();

        var totalCount = await query.CountAsync();

        var roles = await query
            .OrderBy(r => r.Id)
            .Skip((pagination.EnsurePage - 1) * pagination.EnsurePageSize)
            .Take(pagination.EnsurePageSize)
            .ToListAsync();

        var roleIds = roles.Select(r => r.Id).ToList();
        var userCounts = await _db.UserRoles
            .Where(ur => roleIds.Contains(ur.RoleId))
            .GroupBy(ur => ur.RoleId)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToListAsync();

        var countsMap = userCounts.ToDictionary(x => x.RoleId, x => x.Count);

        var items = roles.Select(r => new RoleResponse(
            r.Id, r.Name!, r.Description,
            countsMap.GetValueOrDefault(r.Id, 0))).ToList();

        return new PaginatedResponse<RoleResponse>(items, totalCount, pagination.EnsurePage, pagination.EnsurePageSize);
    }

    public async Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) throw new KeyNotFoundException("Rôle non trouvé");

        role.Description = request.Description ?? role.Description;
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var count = await _db.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
        return new RoleResponse(role.Id, role.Name!, role.Description, count);
    }

    public async Task DeleteAsync(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role != null) await _roleManager.DeleteAsync(role);
    }

    public async Task<RoleResponse?> GetByIdAsync(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return null;
        var count = await _db.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
        return new RoleResponse(role.Id, role.Name!, role.Description, count);
    }

    public async Task AssignToUserAsync(Guid userId, Guid roleId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (user == null || role == null) throw new KeyNotFoundException();

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count != 0)
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role.Name!);
    }

    public async Task RemoveFromUserAsync(Guid userId, Guid roleId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (user == null || role == null) throw new KeyNotFoundException();

        await _userManager.RemoveFromRoleAsync(user, role.Name!);
    }

    public async Task<List<RoleResponse>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new KeyNotFoundException();

        var roleNames = await _userManager.GetRolesAsync(user);
        var roles = await _db.Roles.AsNoTracking()
            .Where(r => roleNames.Contains(r.Name!))
            .ToListAsync();

        return roles.Select(r => new RoleResponse(
            r.Id, r.Name!, r.Description, 0)).ToList();
    }
}
