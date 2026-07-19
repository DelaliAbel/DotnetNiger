using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public partial class AdminService
{
    public async Task<bool> UpdateUserTeamAsync(Guid id, bool isTeamMember, string position)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return false;

        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == id);
        if (member == null)
        {
            member = new Member
            {
                Id = Guid.NewGuid(),
                UserId = id,
                DisplayName = user.FirstName ?? user.Email ?? "",
                IsTeamMember = isTeamMember,
                Position = position
            };
            _db.Members.Add(member);
        }
        else
        {
            member.IsTeamMember = isTeamMember;
            member.Position = position;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReplaceUserRolesAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<DashboardStats> GetDashboardAsync()
    {
        var totalUsers = await _db.Users.IgnoreQueryFilters().CountAsync();
        var totalRoles = await _db.Roles.IgnoreQueryFilters().CountAsync();
        var totalEvents = await _db.Events.IgnoreQueryFilters().CountAsync();
        var totalPosts = await _db.Posts.IgnoreQueryFilters().CountAsync();
        var totalResources = await _db.Resources.IgnoreQueryFilters().CountAsync();
        var totalProjects = await _db.Projects.IgnoreQueryFilters().CountAsync();
        var totalMembers = await _db.Members.IgnoreQueryFilters().CountAsync();
        var pendingEvents = await _db.Events.IgnoreQueryFilters().CountAsync(e => e.Status == EventStatus.PendingReview);
        var pendingPosts = await _db.Posts.IgnoreQueryFilters().CountAsync(p => p.Status == PostStatus.PendingReview);

        return new DashboardStats(
            totalUsers, totalRoles, totalEvents, totalPosts, totalResources,
            totalProjects, totalMembers, pendingEvents, pendingPosts);
    }

    public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var roleExists = await _db.Roles.AnyAsync(r => r.Name == roleName);
        if (!roleExists) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Contains(roleName)) return true;

        if (currentRoles.Count != 0)
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (result.Succeeded)
            await _auditLog.LogAsync("User", userId, "AssignRole", $"Rôle {roleName} assigné (remplace {string.Join(", ", currentRoles)})");
        return result.Succeeded;
    }

    public async Task<bool> RemoveUserRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var roleExists = await _db.Roles.AnyAsync(r => r.Name == roleName);
        if (!roleExists) return false;

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (result.Succeeded)
            await _auditLog.LogAsync("User", userId, "RemoveRole", $"Rôle {roleName} retiré");
        return result.Succeeded;
    }
}
