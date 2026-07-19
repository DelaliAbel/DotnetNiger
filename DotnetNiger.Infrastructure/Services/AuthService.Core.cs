using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Domain.Models;
using DotnetNiger.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetNiger.Infrastructure.Services;

public partial class AuthService
{
    public async Task<(ApplicationUser user, IList<string> roles)> ValidateCredentialsAsync(
        string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect");
        if (!await _userManager.IsEmailConfirmedAsync(user))
            throw new UnauthorizedAccessException("Email non confirmé");

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);
        if (result.IsLockedOut)
            throw new UnauthorizedAccessException("Compte temporairement verrouillé");
        if (result.IsNotAllowed)
            throw new UnauthorizedAccessException("Connexion non autorisée - vérifiez que votre email est confirmé");
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect");

        var roles = await _userManager.GetRolesAsync(user);
        return (user, roles);
    }

    public async Task<UserInfoResponse> LoginAsync(string email, string password, bool rememberMe, string ipAddress, string userAgent)
    {
        ApplicationUser? user = null;
        try
        {
            (user, var roles) = await ValidateCredentialsAsync(email, password);
            await RecordLoginAsync(user.Id, ipAddress, userAgent, true);
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
            return new UserInfoResponse(
                user.Id, user.Email!, user.FirstName, user.LastName, user.AvatarUrl,
                user.IsActive, roles, permissions, rememberMe);
        }
        catch (UnauthorizedAccessException ex)
        {
            if (user != null)
                await RecordLoginAsync(user.Id, ipAddress, userAgent, false, failureReason: ex.Message);
            throw;
        }
    }

    public async Task<(ApplicationUser user, IList<string> roles)> HandleExternalLoginAsync(string provider)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            throw new InvalidOperationException("Erreur lors du login externe");

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);
        if (result.Succeeded)
            return await HandleExistingExternalLoginAsync(info);

        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Email requis pour le login externe");

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
            return await LinkToExistingAccountAsync(existingUser, info);

        return await CreateUserFromExternalLoginAsync(info);
    }

    private async Task<(ApplicationUser user, IList<string> roles)> HandleExistingExternalLoginAsync(ExternalLoginInfo info)
    {
        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        var roles = await _userManager.GetRolesAsync(user!);
        return (user, roles)!;
    }

    private async Task<(ApplicationUser user, IList<string> roles)> LinkToExistingAccountAsync(ApplicationUser existingUser, ExternalLoginInfo info)
    {
        await _userManager.AddLoginAsync(existingUser, info);
        existingUser.EmailConfirmed = true;
        await _userManager.UpdateAsync(existingUser);
        var roles = await _userManager.GetRolesAsync(existingUser);
        return (existingUser, roles);
    }

    private async Task<(ApplicationUser user, IList<string> roles)> CreateUserFromExternalLoginAsync(ExternalLoginInfo info)
    {
        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? throw new InvalidOperationException("Email requis pour le login externe");

        var newUser = new ApplicationUser
        {
            UserName = email, Email = email, EmailConfirmed = true,
            FirstName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value,
            LastName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value
        };
        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
            throw new InvalidOperationException("Erreur création utilisateur");

        await _userManager.AddLoginAsync(newUser, info);
        await _userManager.AddToRoleAsync(newUser, "User");
        return (newUser, new List<string> { "User" });
    }

    public async Task<string> HandleExternalCallbackFrontendAsync(string returnUrl)
    {
        var (user, roles) = await HandleExternalLoginAsync("external");
        var ticket = Guid.NewGuid().ToString("N");
        var cacheEntry = new ExternalLoginTicket
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Roles = roles.ToList(),
            IsActive = user.IsActive
        };
        _cache.Set($"external_login_{ticket}", cacheEntry, TimeSpan.FromMinutes(5));
        var separator = returnUrl.Contains('?') ? '&' : '?';
        return $"{returnUrl}{separator}ticket={ticket}";
    }
}
