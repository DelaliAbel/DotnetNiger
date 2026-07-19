using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetNiger.Infrastructure.Services;

public partial class TwoFactorService
{
    public async Task<(bool success, string[] recoveryCodes)> EnableAsync(Guid userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new InvalidOperationException("User not found");

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);
        if (!isValid)
            throw new InvalidOperationException("Code de vérification invalide");

        var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
        if (!result.Succeeded)
            throw new InvalidOperationException("Impossible d'activer la double authentification");

        if ((await _userManager.CountRecoveryCodesAsync(user)) == 0)
        {
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            return (true, recoveryCodes?.ToArray() ?? []);
        }
        return (true, []);
    }

    public async Task<bool> VerifyCodeAsync(Guid userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        return await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);
    }

    public async Task<TwoFactorStatusResponse> GetStatusAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new InvalidOperationException("User not found");
        var recoveryCodes = await _userManager.CountRecoveryCodesAsync(user);
        return new TwoFactorStatusResponse(user.TwoFactorEnabled, false, recoveryCodes);
    }

    public async Task<string[]> GenerateRecoveryCodesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new InvalidOperationException("User not found");
        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return codes?.ToArray() ?? [];
    }

    public async Task<(bool success, string? error)> DisableAsync(Guid userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return (false, "User not found");
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);
        if (!isValid) return (false, "Code de vérification invalide");
        await _userManager.SetTwoFactorEnabledAsync(user, false);
        return (true, null);
    }

    private bool IsRateLimited(string challengeToken)
    {
        var key = $"2fa_attempts_{challengeToken}";
        var attempts = _cache.Get<int>(key);
        if (attempts >= 5)
            return true;
        _cache.Set(key, attempts + 1, TimeSpan.FromMinutes(1));
        return false;
    }
}
