using System.Security.Claims;
using DotnetNiger.Common.Auth.Responses;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Api.Models;
using DotnetNiger.Identity.Domain.Entities;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetNiger.Identity.Application.Services;

public class TwoFactorVerificationResult
{
    public ClaimsPrincipal? Principal { get; init; }
    public bool RateLimited { get; init; }
    public string? Error { get; init; }

    public static TwoFactorVerificationResult Success(ClaimsPrincipal principal) => new() { Principal = principal };
    public static TwoFactorVerificationResult RateLimitedResult() => new() { RateLimited = true };
    public static TwoFactorVerificationResult Failure(string error) => new() { Error = error };
}

public class TwoFactorService
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
        principal.SetResources("DotnetNiger.Identity.Client");
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
        principal.SetResources("DotnetNiger.Identity.Client");
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
