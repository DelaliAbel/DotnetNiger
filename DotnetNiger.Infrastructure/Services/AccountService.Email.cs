using System.Security.Cryptography;
using System.Text;
using DotnetNiger.Domain.Email;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Infrastructure.Services;

public partial class AccountService
{
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
            var confirmUrl = $"{_smtp.AppBaseUrl}/api/v1/profile/confirm-change-email?email={Uri.EscapeDataString(newEmail)}&code={Uri.EscapeDataString(code)}";
            if (_emailSender is EmailSender typed)
                await typed.SendConfirmationCodeAsync(user, user.Email!, code, confirmUrl);
        }
    }

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

    private async Task SendConfirmationEmailAsync(ApplicationUser user, string code)
    {
        if (string.IsNullOrEmpty(_smtp.Host))
        {
            Console.WriteLine($"[DEV] Code de confirmation pour {user.Email} : {code}");
            return;
        }

        var confirmUrl = $"{_smtp.AppBaseUrl}/api/v1/auth/confirm-email?email={Uri.EscapeDataString(user.Email!)}&code={Uri.EscapeDataString(code)}";

        if (_emailSender is EmailSender typed)
        {
            await typed.SendConfirmationLinkAsync(user, user.Email!, confirmUrl);
            await typed.SendConfirmationCodeAsync(user, user.Email!, code, confirmUrl);
        }
        else
        {
            await _emailSender.SendConfirmationLinkAsync(user, user.Email!, confirmUrl);
        }
    }
}
