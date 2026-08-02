using System.Security.Cryptography;
using System.Text;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Infrastructure.Email;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Api.Application.Services.Users;

/// <summary>Service de gestion de compte utilisateur (inscription, profil, suppression, email).</summary>
public class AccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DotnetNigerDbContext _db;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly SmtpOptions _smtp;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AccountService> _logger;
    private static readonly TimeSpan ProfileCacheDuration = TimeSpan.FromSeconds(60);

    public AccountService(UserManager<ApplicationUser> userManager,
        DotnetNigerDbContext db,
        IEmailSender<ApplicationUser> emailSender,
        IOptions<SmtpOptions> smtp,
        IMemoryCache cache,
        ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _db = db;
        _emailSender = emailSender;
        _smtp = smtp.Value;
        _cache = cache;
        _logger = logger;
    }

    // ============================================================
    // INSCRIPTION
    // ============================================================

    /// <summary>Inscrit un nouvel utilisateur et envoie la confirmation par email.</summary>
    public async Task<ApplicationUser> RegisterAsync(string email, string password,
        string firstName, string lastName, string? phoneNumber = null)
    {
        if (await _userManager.FindByEmailAsync(email) != null)
            throw new InvalidOperationException("Un compte avec cet email existe déjà");

        var user = new ApplicationUser
        {
            UserName = email, Email = email, FirstName = firstName, LastName = lastName,
            PhoneNumber = phoneNumber,
            IsActive = true, EmailConfirmed = false
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

    // ============================================================
    // CONFIRMATION EMAIL
    // ============================================================

    /// <summary>Confirme l'adresse email avec le code de confirmation.</summary>
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

    /// <summary>Renvoie le code de confirmation email.</summary>
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

    // ============================================================
    // CHANGEMENT D'EMAIL
    // ============================================================

    /// <summary>Initie le changement d'email avec envoi d'un code de confirmation.</summary>
    public async Task ChangeEmailAsync(Guid userId, string newEmail)
    {
        var user = await FindUserOrThrowAsync(userId);
        if (string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Le nouvel email est identique à l'email actuel");

        var existing = await _userManager.FindByEmailAsync(newEmail);
        if (existing != null && existing.Id != userId)
            throw new InvalidOperationException("Cet email est déjà utilisé par un autre compte");

        var code = CodeGenerator.Generate();
        user.EmailConfirmationCode = HashCode(code);
        user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        user.PendingEmail = newEmail;
        await _userManager.UpdateAsync(user);

        if (!string.IsNullOrEmpty(_smtp.Host))
        {
            var confirmUrl = $"{_smtp.FrontendBaseUrl.TrimEnd('/')}/verify-email?email={Uri.EscapeDataString(newEmail)}&token={Uri.EscapeDataString(code)}";
            if (_emailSender is EmailSender typed)
                await typed.SendConfirmationCodeAsync(user, user.Email!, code, confirmUrl);
        }
    }

    /// <summary>Confirme le changement d'email avec le code de validation.</summary>
    public async Task ConfirmChangeEmailAsync(Guid userId, string code)
    {
        var user = await FindUserOrThrowAsync(userId);
        if (user.PendingEmail == null)
            throw new InvalidOperationException("Aucun changement d'email en attente");
        if (user.EmailConfirmationCode == null || user.EmailConfirmationCodeExpiry == null)
            throw new InvalidOperationException("Aucun code de confirmation trouvé");
        if (user.EmailConfirmationCodeExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Code de confirmation expiré");

        var hashedCode = HashCode(code);
        if (!string.Equals(user.EmailConfirmationCode, hashedCode, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Code de confirmation invalide");

        var newEmail = user.PendingEmail;
        user.Email = newEmail;
        user.UserName = newEmail;
        user.NormalizedEmail = _userManager.NormalizeEmail(newEmail);
        user.NormalizedUserName = _userManager.NormalizeName(newEmail);
        user.PendingEmail = null;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationCodeExpiry = null;
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
    }

    // ============================================================
    // MOT DE PASSE OUBLIÉ / RÉINITIALISATION
    // ============================================================

    /// <summary>Envoie un lien de réinitialisation de mot de passe.</summary>
    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{_smtp.FrontendBaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
        await _emailSender.SendPasswordResetLinkAsync(user, email, resetLink);
    }

    /// <summary>Réinitialise le mot de passe avec un token.</summary>
    public async Task<string?> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return "INVALID_EMAIL";

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return string.Join(", ", result.Errors.Select(e => e.Description));
        return null;
    }

    /// <summary>Change le mot de passe de l'utilisateur.</summary>
    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await FindUserOrThrowAsync(userId);
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    // ============================================================
    // PROFIL
    // ============================================================

    /// <summary>Récupère le profil utilisateur avec mise en cache.</summary>
    public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
    {
        var cacheKey = $"profile_{userId}";
        if (_cache.TryGetValue(cacheKey, out UserProfileResponse? cached))
            return cached!;

        var user = await FindUserOrThrowAsync(userId);
        var roles = await _userManager.GetRolesAsync(user);
        var profile = new UserProfileResponse(
            user.Id, user.Email!, user.FirstName, user.LastName, user.AvatarUrl,
            roles);

        _cache.Set(cacheKey, profile, ProfileCacheDuration);
        return profile;
    }

    /// <summary>Met à jour le profil utilisateur et invalide le cache.</summary>
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
            roles);
    }

    /// <summary>Supprime le profil utilisateur après purge complète de ses données.</summary>
    public async Task DeleteProfileAsync(Guid userId)
    {
        var user = await FindUserOrThrowAsync(userId);
        await UserDataPurger.PurgeAsync(_db, userId);
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    /// <summary>Vérifie le mot de passe de l'utilisateur (ré-authentification avant suppression définitive).</summary>
    public async Task<bool> VerifyPasswordAsync(Guid userId, string password)
    {
        var user = await FindUserOrThrowAsync(userId);
        return await _userManager.CheckPasswordAsync(user, password);
    }

    /// <summary>Indique si l'utilisateur possède un mot de passe local (compte local, pas uniquement externe).</summary>
    public async Task<bool> HasPasswordAsync(Guid userId)
    {
        var user = await FindUserOrThrowAsync(userId);
        return await _userManager.HasPasswordAsync(user);
    }

    // ============================================================
    // DEMANDE DE SUPPRESSION
    // ============================================================

    /// <summary>Crée une demande de suppression de compte planifiée à 7 jours.</summary>
    public async Task<AccountDeletionRequest> RequestDeletionAsync(Guid userId)
    {
        var existing = await _db.AccountDeletionRequests
            .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsProcessed && d.CancelledAt == null);
        if (existing != null)
            throw new InvalidOperationException("Une demande de suppression est déjà en cours.");

        var request = new AccountDeletionRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RequestedAt = DateTime.UtcNow,
            ScheduledFor = DateTime.UtcNow.AddDays(7)
        };
        _db.AccountDeletionRequests.Add(request);
        await _db.SaveChangesAsync();
        return request;
    }

    /// <summary>Annule une demande de suppression en cours.</summary>
    public async Task<bool> CancelDeletionAsync(Guid userId)
    {
        var request = await _db.AccountDeletionRequests
            .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsProcessed && d.CancelledAt == null);
        if (request == null) return false;

        request.CancelledAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Traite les demandes de suppression dont la date est dépassée.</summary>
    public async Task ProcessPendingDeletionsAsync()
    {
        var now = DateTime.UtcNow;
        var pending = await _db.AccountDeletionRequests
            .Include(d => d.User)
            .Where(d => !d.IsProcessed && d.CancelledAt == null && d.ScheduledFor <= now)
            .ToListAsync();

        foreach (var request in pending)
        {
            try
            {
                if (request.User != null)
                {
                    await UserDataPurger.PurgeAsync(_db, request.User.Id);
                    await _userManager.DeleteAsync(request.User);
                }
                request.IsProcessed = true;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Échec du traitement de la demande de suppression {DeletionRequestId}", request.Id);
            }
        }
    }

    // ============================================================
    // UTILITAIRES
    // ============================================================

    private static string HashCode(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

    private async Task<ApplicationUser> FindUserOrThrowAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user ?? throw new InvalidOperationException("Utilisateur non trouvé");
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user, string code)
    {
        if (string.IsNullOrEmpty(_smtp.Host))
        {
            _logger.LogWarning(
                "[DEV] SMTP non configuré — code de confirmation affiché uniquement pour le développement | Email: {Email}",
                user.Email);
            return;
        }

        var confirmUrl = $"{_smtp.FrontendBaseUrl.TrimEnd('/')}/verify-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(code)}";

        if (_emailSender is EmailSender typed)
        {
            await typed.SendConfirmationCodeAsync(user, user.Email!, code, confirmUrl);
        }
        else
        {
            await _emailSender.SendConfirmationLinkAsync(user, user.Email!, confirmUrl);
        }
    }
}
