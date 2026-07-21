using System.Security.Claims;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace DotnetNiger.Api.Services.Auth;

public class AuthorizeResult
{
    public ClaimsPrincipal? Principal { get; init; }
    public bool RequiresChallenge { get; init; }
    public string? Provider { get; init; }

    public static AuthorizeResult Success(ClaimsPrincipal principal) => new() { Principal = principal };
    public static AuthorizeResult Challenge(string? provider = null) => new() { RequiresChallenge = true, Provider = provider };
}

public class OidcService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IPermissionService _permissionService;

    public OidcService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IPermissionService permissionService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _permissionService = permissionService;
    }

    public async Task<AuthorizeResult> AuthorizeAsync(HttpContext context)
    {
        var request = context.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("La requête OpenID Connect est introuvable.");

        var appResult = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (appResult.Succeeded)
        {
            var user = await _userManager.GetUserAsync(appResult.Principal);
            if (user != null && user.IsActive)
                return await CreateAuthorizeResultAsync(user, request);
        }

        if (!string.IsNullOrEmpty(request.IdentityProvider))
        {
            var extResult = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (extResult.Succeeded)
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info != null)
                {
                    var user = await ProcessExternalLoginAsync(info);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return await CreateAuthorizeResultAsync(user, request);
                }
            }

            return AuthorizeResult.Challenge(request.IdentityProvider);
        }

        return AuthorizeResult.Challenge();
    }

    private async Task<ApplicationUser> ProcessExternalLoginAsync(ExternalLoginInfo info)
    {
        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);

        if (result.Succeeded)
        {
            var existing = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existing != null && existing.IsActive)
                return existing;
        }

        var email = info.Principal.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("L'email est requis pour se connecter via un fournisseur externe");

        var userByEmail = await _userManager.FindByEmailAsync(email);
        if (userByEmail != null)
        {
            await _userManager.AddLoginAsync(userByEmail, info);
            userByEmail.EmailConfirmed = true;
            await _userManager.UpdateAsync(userByEmail);
            return userByEmail;
        }

        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = info.Principal.FindFirst(ClaimTypes.GivenName)?.Value,
            LastName = info.Principal.FindFirst(ClaimTypes.Surname)?.Value
        };

        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Erreur création utilisateur: {errors}");
        }

        await _userManager.AddLoginAsync(newUser, info);
        await _userManager.AddToRoleAsync(newUser, "User");
        return newUser;
    }

    private async Task<AuthorizeResult> CreateAuthorizeResultAsync(ApplicationUser user, OpenIddictRequest request)
    {
        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        principal.SetClaim(Claims.Subject, user.Id.ToString());

        var scopes = (request.Scope ?? "openid profile email roles offline_access")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        principal.SetScopes(scopes);

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
        TokenPrincipalBuilder.SetUserClaims(principal, user, roles, permissions);
        TokenPrincipalBuilder.SetCommonDestinations(principal);

        return AuthorizeResult.Success(principal);
    }

    public async Task<UserInfoResponse> GetUserInfoAsync(ClaimsPrincipal userPrincipal)
    {
        var userId = userPrincipal.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (userId == null)
            throw new UnauthorizedAccessException();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException();

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
        return new UserInfoResponse(
            user.Id, user.Email!, user.FirstName, user.LastName, user.AvatarUrl,
            user.IsActive, roles, permissions);
    }
}
