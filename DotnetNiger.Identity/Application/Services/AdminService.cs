using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DotnetNiger.Common.Constants;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Common.Email;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services;

public class AdminService : IAdminService
{
    private readonly IdentityDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly SmtpOptions _smtp;
    private readonly IAuditLogService _auditLog;

    public AdminService(IdentityDbContext db, UserManager<ApplicationUser> userManager,
        IEmailSender<ApplicationUser> emailSender, IOptions<SmtpOptions> smtp,
        IAuditLogService auditLog)
    {
        _db = db;
        _userManager = userManager;
        _emailSender = emailSender;
        _smtp = smtp.Value;
        _auditLog = auditLog;
    }

    public async Task InviteAsync(string email, string role)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
            throw new InvalidOperationException(ErrorMessages.UserAlreadyExists);

        var tenant = await _db.Tenants.FirstOrDefaultAsync();
        if (tenant == null)
            throw new InvalidOperationException(ErrorMessages.TenantNotFound);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            TenantId = tenant.Id,
            IsActive = true,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, GenerateTemporaryPassword());
        if (!result.Succeeded)
            throw new InvalidOperationException($"Erreur: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await _userManager.AddToRoleAsync(user, role);
        await _auditLog.LogAsync("User", user.Id, "Invite", $"Invitation envoyée à {email} avec le rôle {role}");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var inviteUrl = $"{_smtp.AppBaseUrl.TrimEnd('/')}/Account/Register?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
        if (_emailSender is EmailSender typed)
            await typed.SendInviteEmailAsync(email, inviteUrl, role);
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
        var data = new byte[16];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(data);
        return new string(data.Select(b => chars[b % chars.Length]).ToArray()) + "Aa1!";
    }

    public async Task<List<UserResponse>> GetAllUsersAcrossTenantsAsync()
    {
        var users = await _db.Users.IgnoreQueryFilters()
            .OrderBy(u => u.Email)
            .ToListAsync();

        var userIds = users.Select(u => u.Id).ToList();
        var roleMappings = userIds.Count == 0
            ? []
            : await _db.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name! })
                .ToListAsync();

        var rolesByUser = roleMappings
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

        return users.Select(u => new UserResponse(
            u.Id, u.Email!, u.FirstName, u.LastName, u.AvatarUrl,
            u.TenantId, u.IsActive, u.EmailConfirmed, u.CreatedAt,
            rolesByUser.GetValueOrDefault(u.Id, []))).ToList();
    }

    public async Task<bool> UpdateUserStatusAsync(Guid id, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return false;

        user.IsActive = isActive;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
            await _auditLog.LogAsync("User", id, isActive ? "Activate" : "Deactivate");
        return result.Succeeded;
    }

    public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var roleExists = await _db.Roles.AnyAsync(r => r.Name == roleName);
        if (!roleExists) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Contains(roleName)) return true;

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (result.Succeeded)
            await _auditLog.LogAsync("User", userId, "AssignRole", $"Rôle {roleName} assigné");
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

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return false;
        var email = user.Email;
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
            await _auditLog.LogAsync("User", id, "Delete", $"Utilisateur {email} supprimé");
        return result.Succeeded;
    }

    /// <summary>Retourne un utilisateur avec ses rôles.</summary>
    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserResponse(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.AvatarUrl, user.TenantId, user.IsActive,
            user.EmailConfirmed, user.CreatedAt, roles.ToList());
    }

    /// <summary>Met à jour le profil d'un utilisateur (nom, avatar).</summary>
    public async Task<UserResponse?> UpdateUserProfileAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return null;

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        return new UserResponse(user.Id, user.Email!, user.FirstName, user.LastName,
            user.AvatarUrl, user.TenantId, user.IsActive, user.EmailConfirmed,
            user.CreatedAt, roles.ToList());
    }

    /// <summary>Crée un utilisateur (admin).</summary>
    public async Task<UserResponse?> CreateUserAsync(AdminCreateUserRequest request)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync();
        if (tenant == null) return null;

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName ?? ".",
            TenantId = tenant.Id,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, request.Role ?? RoleConstants.User);
        await _auditLog.LogAsync("User", user.Id, "Create", $"Utilisateur {request.Email} créé par admin");

        var roles = await _userManager.GetRolesAsync(user);
        return new UserResponse(user.Id, user.Email!, user.FirstName, user.LastName,
            user.AvatarUrl, user.TenantId, user.IsActive, user.EmailConfirmed,
            user.CreatedAt, roles.ToList());
    }
}
