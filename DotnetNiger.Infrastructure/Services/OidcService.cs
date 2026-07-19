using System.Security.Claims;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Models;
using DotnetNiger.Domain.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace DotnetNiger.Infrastructure.Services;

public class AuthorizeResult
{
    public ClaimsPrincipal? Principal { get; init; }
    public bool RequiresChallenge { get; init; }

    public static AuthorizeResult Success(ClaimsPrincipal principal) => new() { Principal = principal };
    public static AuthorizeResult Challenge() => new() { RequiresChallenge = true };
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

        var result = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded)
            return AuthorizeResult.Challenge();

        var user = await _userManager.GetUserAsync(result.Principal);
        if (user == null || !user.IsActive)
            return AuthorizeResult.Challenge();

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        principal.SetClaim(Claims.Subject, user.Id.ToString());

        var scopes = (request.Scope ?? "openid profile email roles offline_access")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        principal.SetScopes(scopes);

        var roles = await _userManager.GetRolesAsync(user);
        TokenPrincipalBuilder.SetUserClaims(principal, user, roles);
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
