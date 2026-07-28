using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Infrastructure.Email;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Users;

/// <summary>Service de gestion des utilisateurs (CRUD, mots de passe, rôles).</summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DotnetNigerDbContext _db;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly SmtpOptions _smtp;

    public UserService(UserManager<ApplicationUser> userManager, DotnetNigerDbContext db,
        IEmailSender<ApplicationUser> emailSender, IOptions<SmtpOptions> smtp)
    {
        _userManager = userManager;
        _db = db;
        _emailSender = emailSender;
        _smtp = smtp.Value;
    }

    /// <summary>Crée un nouvel utilisateur avec un mot de passe et un rôle optionnel.</summary>
    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email, Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            AvatarUrl = request.AvatarUrl
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (request.Roles?.Any() == true)
        {
            var singleRole = request.Roles.First();
            await _userManager.AddToRoleAsync(user, singleRole);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return MapToResponse(user, roles);
    }

    /// <summary>Récupère un utilisateur par son identifiant avec ses rôles.</summary>
    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return null;
        var roles = await _userManager.GetRolesAsync(user);
        return MapToResponse(user, roles);
    }

    /// <summary>Récupère la liste paginée des utilisateurs.</summary>
    public async Task<PaginatedResponse<UserResponse>> GetAllAsync(PaginationQuery pagination)
    {
        var query = _db.Users.AsNoTracking();
        var total = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.Email)
            .Skip((pagination.EnsurePage - 1) * pagination.EnsurePageSize)
            .Take(pagination.EnsurePageSize)
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

        return new PaginatedResponse<UserResponse>(
            users.Select(u => MapToResponse(u, rolesByUser.GetValueOrDefault(u.Id, []))).ToList(),
            total, pagination.EnsurePage, pagination.EnsurePageSize);
    }

    /// <summary>Met à jour le profil d'un utilisateur.</summary>
    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) throw new KeyNotFoundException("Utilisateur non trouvé");

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var roles = await _userManager.GetRolesAsync(user);
        return MapToResponse(user, roles);
    }

    /// <summary>Supprime un utilisateur par son identifiant.</summary>
    public async Task DeleteAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null) await _userManager.DeleteAsync(user);
    }

    /// <summary>Change le mot de passe d'un utilisateur.</summary>
    public async Task<UserResponse> ChangePasswordAsync(Guid id, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) throw new KeyNotFoundException("Utilisateur non trouvé");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var roles = await _userManager.GetRolesAsync(user);
        return MapToResponse(user, roles);
    }

    /// <summary>Envoie un email de réinitialisation de mot de passe.</summary>
    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{_smtp.FrontendBaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        await _emailSender.SendPasswordResetLinkAsync(user, email, resetLink);
    }

    /// <summary>Réinitialise le mot de passe avec un token de confirmation.</summary>
    public async Task ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) throw new KeyNotFoundException("Utilisateur non trouvé");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    private static UserResponse MapToResponse(ApplicationUser user, IList<string> roles) => new(
        user.Id, user.Email!, user.FirstName, user.LastName, user.AvatarUrl,
        user.IsActive, user.EmailConfirmed, user.CreatedAt, roles);
}
