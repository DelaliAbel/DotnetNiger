using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetNiger.Infrastructure.Services;

public partial class TwoFactorService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMemoryCache _cache;

    public TwoFactorService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IMemoryCache cache)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _cache = cache;
    }

    public async Task<TwoFactorVerificationResult> VerifyAsync(
        string code, string challengeToken, HttpRequest request)
    {
        if (IsRateLimited(challengeToken))
            return TwoFactorVerificationResult.RateLimitedResult();

        if (!_cache.TryGetValue($"2fa_challenge_{challengeToken}", out TwoFactorChallenge? challenge) || challenge == null)
            return TwoFactorVerificationResult.Failure("Jeton de vérification invalide ou expiré");

        if (challenge.ExpiresAt < DateTime.UtcNow)
        {
            _cache.Remove($"2fa_challenge_{challengeToken}");
            return TwoFactorVerificationResult.Failure("Jeton de vérification expiré");
        }

        var user = await _userManager.FindByIdAsync(challenge.UserId.ToString());
        if (user == null)
            return TwoFactorVerificationResult.Failure("Utilisateur non trouvé");

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);

        if (!isValid)
            return TwoFactorVerificationResult.Failure("Code de vérification invalide");

        _cache.Remove($"2fa_challenge_{challengeToken}");

        var roles = await _userManager.GetRolesAsync(user);
        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        TokenPrincipalBuilder.SetUserClaims(principal, user, roles);
        TokenPrincipalBuilder.SetUserScopes(principal, request);

        TokenPrincipalBuilder.SetCommonDestinations(principal);

        return TwoFactorVerificationResult.Success(principal);
    }

    public async Task<TwoFactorVerificationResult> VerifyRecoveryAsync(
        string recoveryCode, string challengeToken, HttpRequest request)
    {
        if (IsRateLimited(challengeToken))
            return TwoFactorVerificationResult.RateLimitedResult();

        if (!_cache.TryGetValue($"2fa_challenge_{challengeToken}", out TwoFactorChallenge? challenge) || challenge == null)
            return TwoFactorVerificationResult.Failure("Jeton de vérification invalide ou expiré");

        if (challenge.ExpiresAt < DateTime.UtcNow)
        {
            _cache.Remove($"2fa_challenge_{challengeToken}");
            return TwoFactorVerificationResult.Failure("Jeton de vérification expiré");
        }

        var user = await _userManager.FindByIdAsync(challenge.UserId.ToString());
        if (user == null)
            return TwoFactorVerificationResult.Failure("Utilisateur non trouvé");

        var recoveryResult = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, recoveryCode);
        if (!recoveryResult.Succeeded)
            return TwoFactorVerificationResult.Failure("Code de récupération invalide");

        _cache.Remove($"2fa_challenge_{challengeToken}");

        var roles = await _userManager.GetRolesAsync(user);
        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        TokenPrincipalBuilder.SetUserClaims(principal, user, roles);
        TokenPrincipalBuilder.SetUserScopes(principal, request);

        TokenPrincipalBuilder.SetCommonDestinations(principal);

        return TwoFactorVerificationResult.Success(principal);
    }

    public async Task<bool> RequiresTwoFactorAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;
        return user.TwoFactorEnabled;
    }

    public async Task<(string sharedKey, string authenticatorUri)> GetSetupAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new InvalidOperationException("User not found");

        await _userManager.ResetAuthenticatorKeyAsync(user);
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }
        var sharedKey = string.Join(' ', unformattedKey!.Chunk(4).Select(c => new string(c)));
        var email = await _userManager.GetEmailAsync(user);
        var appName = "DotnetNiger";
        var authenticatorUri = $"otpauth://totp/{appName}:{Uri.EscapeDataString(email!)}?secret={unformattedKey}&issuer={appName}&digits=6";
        return (sharedKey, authenticatorUri);
    }
}
