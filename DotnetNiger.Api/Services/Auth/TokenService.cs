using System.Security.Claims;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace DotnetNiger.Api.Services.Auth;

public class TokenService
{
    private readonly AuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IPermissionService _permissionService;

    public TokenService(
        AuthService authService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IPermissionService permissionService)
    {
        _authService = authService;
        _userManager = userManager;
        _signInManager = signInManager;
        _permissionService = permissionService;
    }

    public async Task<TokenExchangeResult> HandleTokenExchangeAsync(HttpRequest request)
    {
        var grantType = request.Form["grant_type"].FirstOrDefault();
        return grantType switch
        {
            "refresh_token" => await HandleRefreshTokenAsync(request.HttpContext!),
            "authorization_code" => await HandleAuthorizationCodeAsync(request.HttpContext!),
            "password" => await HandlePasswordAsync(request),
            _ => TokenExchangeResult.Failure("Unsupported grant type")
        };
    }

    private async Task<TokenExchangeResult> HandleRefreshTokenAsync(HttpContext context)
    {
        var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (result?.Principal == null)
            return TokenExchangeResult.Failure("The refresh token is invalid");
        var userId = result.Principal.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (userId != null)
        {
            var refreshUser = await _userManager.FindByIdAsync(userId);
            if (refreshUser == null || !refreshUser.IsActive)
                return TokenExchangeResult.Failure("User no longer exists or is inactive");
        }
        return TokenExchangeResult.Success(result.Principal);
    }

    private async Task<TokenExchangeResult> HandleAuthorizationCodeAsync(HttpContext context)
    {
        var principal = (await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        if (principal == null)
            return TokenExchangeResult.Failure("The authorization code is invalid");
        return TokenExchangeResult.Success(principal);
    }

    private async Task<TokenExchangeResult> HandlePasswordAsync(HttpRequest request)
    {
        var username = request.Form["username"].FirstOrDefault();
        var password = request.Form["password"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return TokenExchangeResult.Failure("Username and password are required");

        try
        {
            var (loginUser, roles) = await _authService.ValidateCredentialsAsync(username, password);
            var loginPrincipal = await _signInManager.CreateUserPrincipalAsync(loginUser);
            var loginPermissions = await _permissionService.GetUserPermissionsAsync(loginUser.Id);
            TokenPrincipalBuilder.SetUserClaims(loginPrincipal, loginUser, roles, loginPermissions);
            var scopes = request.Form["scope"];
            loginPrincipal.SetScopes(scopes.Count > 0
                ? scopes.SelectMany(s => (s ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries))
                : ["openid", "profile", "email", "roles"]);

            var rememberMe = string.Equals(request.Form["remember_me"].FirstOrDefault(), "true",
                StringComparison.OrdinalIgnoreCase);
            loginPrincipal.SetAccessTokenLifetime(
                rememberMe ? TimeSpan.FromDays(7) : TimeSpan.FromHours(1));
            TokenPrincipalBuilder.SetCommonDestinations(loginPrincipal);
            return TokenExchangeResult.Success(loginPrincipal);
        }
        catch (UnauthorizedAccessException ex)
        {
            return TokenExchangeResult.Failure(ex.Message);
        }
    }
}
