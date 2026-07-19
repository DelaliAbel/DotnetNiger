using System.Security.Cryptography;
using System.Text;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Common.Email;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Application.Services;

public partial class AccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityDbContext _db;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly SmtpOptions _smtp;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan ProfileCacheDuration = TimeSpan.FromSeconds(60);

    public AccountService(UserManager<ApplicationUser> userManager,
        IdentityDbContext db,
        IEmailSender<ApplicationUser> emailSender,
        IOptions<SmtpOptions> smtp,
        IMemoryCache cache)
    {
        _userManager = userManager;
        _db = db;
        _emailSender = emailSender;
        _smtp = smtp.Value;
        _cache = cache;
    }

    public async Task<ApplicationUser> RegisterAsync(string email, string password,
        string firstName, string lastName, Guid? tenantId = null)
    {
        if (await _userManager.FindByEmailAsync(email) != null)
            throw new InvalidOperationException("Un compte avec cet email existe déjà");

        var tenant = tenantId.HasValue
            ? await _db.Tenants.FindAsync(tenantId.Value)
            : await _db.Tenants.FirstOrDefaultAsync();
        if (tenant == null)
            throw new InvalidOperationException("Aucun tenant trouvé");

        var user = new ApplicationUser
        {
            UserName = email, Email = email, FirstName = firstName, LastName = lastName,
            TenantId = tenant.Id, IsActive = true, EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Erreur création: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await _userManager.AddToRoleAsync(user, "User");
        var code = CodeGenerator.Generate();
        user.EmailConfirmationCode = HashCode(code);
        user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        await _userManager.UpdateAsync(user);
        await SendConfirmationEmailAsync(user, code);
        return user;
    }

    public async Task ConfirmEmailAsync(string email, string code)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) throw new InvalidOperationException("Utilisateur non trouvé");
        if (user.EmailConfirmed) throw new InvalidOperationException("Email déjà confirmé");
        if (user.EmailConfirmationCode == null || user.EmailConfirmationCodeExpiry == null)
            throw new InvalidOperationException("Aucun code de confirmation trouvé");
        if (user.EmailConfirmationCodeExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Code de confirmation expiré");

        var hashedCode = HashCode(code);
        if (!string.Equals(user.EmailConfirmationCode, hashedCode, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Code de confirmation invalide");

        user.EmailConfirmed = true;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationCodeExpiry = null;
        await _userManager.UpdateAsync(user);
    }

    public async Task ResendConfirmationCodeAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) throw new InvalidOperationException("Utilisateur non trouvé");
        if (user.EmailConfirmed) throw new InvalidOperationException("Email déjà confirmé");

        var code = CodeGenerator.Generate();
        user.EmailConfirmationCode = HashCode(code);
        user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        await _userManager.UpdateAsync(user);
        await SendConfirmationEmailAsync(user, code);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{_smtp.AppBaseUrl.TrimEnd('/')}/Account/ResetPassword?email={Uri.EscapeDataString(email)}&code={Uri.EscapeDataString(token)}";
        await _emailSender.SendPasswordResetLinkAsync(user, email, resetLink);
    }

    public async Task<string?> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return "INVALID_EMAIL";

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return string.Join(", ", result.Errors.Select(e => e.Description));
        return null;
    }

    private static string HashCode(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

    private async Task<ApplicationUser> FindUserOrThrowAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user ?? throw new InvalidOperationException("Utilisateur non trouvé");
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
    {
        var cacheKey = $"profile_{userId}";
        if (_cache.TryGetValue(cacheKey, out UserProfileResponse? cached))
            return cached!;

        var user = await FindUserOrThrowAsync(userId);
        var roles = await _userManager.GetRolesAsync(user);
        var profile = new UserProfileResponse(
            user.Id, user.Email!, user.FirstName, user.LastName, user.AvatarUrl,
            user.TenantId, roles);

        _cache.Set(cacheKey, profile, ProfileCacheDuration);
        return profile;
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await FindUserOrThrowAsync(userId);
        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        await _userManager.UpdateAsync(user);

        _cache.Remove($"profile_{userId}");

        var roles = await _userManager.GetRolesAsync(user);
        return new UserProfileResponse(
            user.Id, user.Email!, user.FirstName, user.LastName, user.AvatarUrl,
            user.TenantId, roles);
    }

    public async Task DeleteProfileAsync(Guid userId)
    {
        var user = await FindUserOrThrowAsync(userId);
        await _userManager.DeleteAsync(user);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await FindUserOrThrowAsync(userId);
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }


}
