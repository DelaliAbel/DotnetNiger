using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Data.Email;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Services.General;
using DotnetNiger.Api.Data;
using DotnetNiger.Api.DTOs.Requests;

namespace DotnetNiger.Api.Services.Admin;

/// <summary>Service d'administration pour la gestion des utilisateurs, rôles, équipes et tableau de bord.</summary>
public class AdminService : IAdminService
{
    private readonly DotnetNigerDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly SmtpOptions _smtp;
    private readonly IAuditLogService _auditLog;

    public AdminService(DotnetNigerDbContext db, UserManager<ApplicationUser> userManager,
        IEmailSender<ApplicationUser> emailSender, IOptions<SmtpOptions> smtp,
        IAuditLogService auditLog)
    {
        _db = db;
        _userManager = userManager;
        _emailSender = emailSender;
        _smtp = smtp.Value;
        _auditLog = auditLog;
    }

    // ============================================================
    // INVITATION
    // ============================================================

    /// <summary>Envoie une invitation par email avec un rôle assigné.</summary>
    public async Task InviteAsync(string email, string role)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
            throw new InvalidOperationException(ErrorMessages.UserAlreadyExists);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            IsActive = true,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, GenerateTemporaryPassword());
        if (!result.Succeeded)
            throw new InvalidOperationException($"Erreur: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await _userManager.AddToRoleAsync(user, role);
        await _auditLog.LogAsync("User", user.Id, "Invite", $"Invitation envoyée à {email} avec le rôle {role}");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var inviteUrl = $"{_smtp.FrontendBaseUrl.TrimEnd('/')}/verify-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
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

    // ============================================================
    // GESTION DES UTILISATEURS
    // ============================================================

    /// <summary>Met à jour le statut actif/inactif d'un utilisateur.</summary>
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

    /// <summary>Récupère la liste de tous les utilisateurs avec leurs rôles.</summary>
    public async Task<List<UserResponse>> GetAllUsersAsync()
    {
        var users = await _db.Users.AsNoTracking()
            .OrderBy(u => u.Email)
            .ToListAsync();

        var userIds = users.Select(u => u.Id).ToList();
        var roleMappings = await _db.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name! })
            .ToListAsync();

        var rolesByUser = roleMappings
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

        var teamMemberUserIds = await _db.Members
            .Where(m => m.IsTeamMember)
            .Select(m => m.UserId)
            .ToListAsync();
        var teamMemberSet = new HashSet<Guid>(teamMemberUserIds);

        return users.Select(u => new UserResponse(
            u.Id, u.Email!, u.FirstName, u.LastName,
            u.AvatarUrl, u.IsActive, u.EmailConfirmed,
            u.CreatedAt, rolesByUser.GetValueOrDefault(u.Id, []),
            teamMemberSet.Contains(u.Id))).ToList();
    }

    /// <summary>Récupère la liste des utilisateurs (alias de GetAllUsers).</summary>
    public Task<List<UserResponse>> GetUsersAsync() => GetAllUsersAsync();

    /// <summary>Récupère un utilisateur par son identifiant.</summary>
    public Task<UserResponse?> GetUserAsync(Guid id) => GetUserByIdAsync(id);

    /// <summary>Crée un utilisateur avec le rôle admin par défaut.</summary>
    public async Task<UserResponse?> CreateUserAsync(CreateAdminUserRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FullName,
            LastName = ".",
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, "Admin@12345");
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, RoleConstants.Admin);

        var roles = await _userManager.GetRolesAsync(user);
        return new UserResponse(user.Id, user.Email!, user.FirstName, user.LastName,
            user.AvatarUrl, user.IsActive, user.EmailConfirmed,
            user.CreatedAt, roles.ToList());
    }

    /// <summary>Supprime un utilisateur. Un admin ne peut pas supprimer un autre admin.
    /// Un utilisateur ne peut pas se supprimer lui-même.</summary>
    public async Task<bool> DeleteUserAsync(Guid id, Guid? callerId = null)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return false;

        if (callerId.HasValue && callerId.Value == id)
            throw new InvalidOperationException("Vous ne pouvez pas supprimer votre propre compte.");

        var targetRoles = await _userManager.GetRolesAsync(user);
        var isTargetAdmin = targetRoles.Any(r =>
            r == RoleConstants.Admin || r == RoleConstants.SuperAdmin);

        if (isTargetAdmin)
            throw new InvalidOperationException(
                "Un administrateur ne peut pas supprimer un autre administrateur. Seul le SuperAdmin peut effectuer cette action.");

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
            user.AvatarUrl, user.IsActive,
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
            user.AvatarUrl, user.IsActive, user.EmailConfirmed,
            user.CreatedAt, roles.ToList());
    }

    /// <summary>Crée un utilisateur (admin).</summary>
    public async Task<UserResponse?> CreateUserAsync(AdminCreateUserRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName ?? ".",
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, request.Role ?? RoleConstants.User);

        if (request.IsTeamMember)
        {
            var member = new Member
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                DisplayName = request.FirstName ?? request.Email,
                IsTeamMember = true,
                Position = request.Position ?? string.Empty
            };
            _db.Members.Add(member);
            await _db.SaveChangesAsync();
        }

        await _auditLog.LogAsync("User", user.Id, "Create", $"Utilisateur {request.Email} créé par admin");

        var roles = await _userManager.GetRolesAsync(user);
        return new UserResponse(user.Id, user.Email!, user.FirstName, user.LastName,
            user.AvatarUrl, user.IsActive, user.EmailConfirmed,
            user.CreatedAt, roles.ToList());
    }

    // ============================================================
    // ÉQUIPES ET RÔLES
    // ============================================================

    /// <summary>Met à jour le statut d'appartenance à l'équipe d'un utilisateur.</summary>
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

    /// <summary>Remplace tous les rôles d'un utilisateur par un nouveau rôle.</summary>
    public async Task<bool> ReplaceUserRolesAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    /// <summary>Assigne un rôle à un utilisateur (ajoute sans remplacer les rôles existants).</summary>
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
            await _auditLog.LogAsync("User", userId, "AssignRole", $"Rôle {roleName} ajouté (rôles existants : {string.Join(", ", currentRoles)})");
        return result.Succeeded;
    }

    /// <summary>Retire un rôle spécifique à un utilisateur.</summary>
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

    // ============================================================
    // TABLEAU DE BORD
    // ============================================================

    /// <summary>Récupère les statistiques globales du tableau de bord.</summary>
    public async Task<DashboardStats> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;

        var totalPosts = await _db.Posts.IgnoreQueryFilters().CountAsync();
        var publishedPosts = await _db.Posts.IgnoreQueryFilters().CountAsync(p => p.Status == PostStatus.Published);
        var draftPosts = await _db.Posts.IgnoreQueryFilters().CountAsync(p => p.Status == PostStatus.Draft);
        var totalEvents = await _db.Events.IgnoreQueryFilters().CountAsync();
        var upcomingEvents = await _db.Events.IgnoreQueryFilters().CountAsync(e => e.StartDate > now);
        var pastEvents = await _db.Events.IgnoreQueryFilters().CountAsync(e => e.EndDate < now);
        var pendingEvents = await _db.Events.IgnoreQueryFilters().CountAsync(e => e.Status == EventStatus.PendingReview);
        var totalResources = await _db.Resources.IgnoreQueryFilters().CountAsync();
        var totalResourceViews = await _db.Resources.IgnoreQueryFilters().SumAsync(r => r.ViewCount);
        var membersCount = await _db.Members.IgnoreQueryFilters().CountAsync();
        var activeNewsletter = await _db.NewsletterSubscriptions.IgnoreQueryFilters().CountAsync(s => s.IsActive);
        var commentsCount = await _db.Comments.IgnoreQueryFilters().CountAsync();
        var projectsCount = await _db.Projects.IgnoreQueryFilters().CountAsync();
        var partnersCount = await _db.Partners.IgnoreQueryFilters().CountAsync();
        var pendingCertificates = await _db.Certificates.IgnoreQueryFilters().CountAsync(c => c.Status == "Pending");

        return new DashboardStats(
            totalPosts, publishedPosts, draftPosts,
            totalEvents, upcomingEvents, pastEvents, pendingEvents,
            totalResources, totalResourceViews,
            membersCount, activeNewsletter, commentsCount,
            projectsCount, partnersCount, pendingCertificates);
    }

    /// <summary>Récupère les statistiques personnelles d'un collaborateur.</summary>
    public async Task<DashboardStats> GetCollaboratorDashboardAsync(Guid userId)
    {
        var now = DateTime.UtcNow;

        var myPosts = await _db.Posts.CountAsync(p => p.AuthorId == userId && !p.IsDeleted);
        var myPublishedPosts = await _db.Posts.CountAsync(p => p.AuthorId == userId && !p.IsDeleted && p.Status == PostStatus.Published);
        var myDraftPosts = await _db.Posts.CountAsync(p => p.AuthorId == userId && !p.IsDeleted && p.Status == PostStatus.Draft);
        var myEvents = await _db.Events.CountAsync(e => e.OrganizerId == userId);
        var myUpcomingEvents = await _db.Events.CountAsync(e => e.OrganizerId == userId && e.StartDate > now);
        var myPastEvents = await _db.Events.CountAsync(e => e.OrganizerId == userId && e.EndDate < now);
        var myPendingEvents = await _db.Events.CountAsync(e => e.OrganizerId == userId && e.Status == EventStatus.PendingReview);
        var myResources = await _db.Resources.CountAsync(r => r.AuthorId == userId);
        var myResourceViews = await _db.Resources.Where(r => r.AuthorId == userId).SumAsync(r => r.ViewCount);
        var myProjects = await _db.Projects.CountAsync(p => p.CreatedBy == userId);
        var myPendingCertificates = await _db.Certificates.CountAsync(c => c.UserId == userId && c.Status == "Pending");

        return new DashboardStats(
            myPosts, myPublishedPosts, myDraftPosts,
            myEvents, myUpcomingEvents, myPastEvents, myPendingEvents,
            myResources, myResourceViews,
            0, 0, 0,
            myProjects, 0, myPendingCertificates);
    }
}
