using System.Security.Claims;
using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Models;
using DotnetNiger.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace DotnetNiger.Infrastructure.Services;

public class TokenService
{
    private readonly AuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMemoryCache _cache;

    public TokenService(
        AuthService authService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IMemoryCache cache)
    {
        _authService = authService;
        _userManager = userManager;
        _signInManager = signInManager;
        _cache = cache;
    }

    public void SetupRefreshTokenContext(HttpContext context, string refreshToken)
    {
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = "web-ui",
            ["scope"] = "openid profile email roles offline_access"
        });
    }

    public async Task<TokenExchangeResult> HandleTokenExchangeAsync(HttpRequest request)
    {
        var grantType = request.Form["grant_type"].FirstOrDefault();
        return grantType switch
        {
            "client_credentials" => await HandleClientCredentialsAsync(request),
            "refresh_token" => await HandleRefreshTokenAsync(request.HttpContext!),
            "authorization_code" => await HandleAuthorizationCodeAsync(request.HttpContext!),
            "external_login" => await HandleExternalLoginAsync(request),
            "password" => await HandlePasswordAsync(request),
            _ => TokenExchangeResult.Failure("Unsupported grant type")
        };
    }

    private async Task<TokenExchangeResult> HandleClientCredentialsAsync(HttpRequest request)
    {
        var clientId = request.Form["client_id"].FirstOrDefault();
        if (string.IsNullOrEmpty(clientId))
            return TokenExchangeResult.Failure("client_id is required");

        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role);

        identity.AddClaim(OpenIddictConstants.Claims.Subject, clientId);
        identity.AddClaim(OpenIddictConstants.Claims.Name, clientId);
        identity.AddClaim("client_id", clientId);
        identity.AddClaim(ClaimTypes.Role, RoleConstants.Client);

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(request.Form["scope"].SelectMany(
            s => (s ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)));
        principal.SetDestinations(claim => claim.Type switch
        {
            OpenIddictConstants.Claims.Subject => [OpenIddictConstants.Destinations.AccessToken],
            OpenIddictConstants.Claims.Name => [OpenIddictConstants.Destinations.AccessToken],
            "client_id" => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],
            ClaimTypes.Role => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],
            _ => [OpenIddictConstants.Destinations.AccessToken],
        });
        return TokenExchangeResult.Success(principal);
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

    private async Task<TokenExchangeResult> HandleExternalLoginAsync(HttpRequest request)
    {
        var ticket = request.Form["ticket"].FirstOrDefault();
        if (string.IsNullOrEmpty(ticket))
            return TokenExchangeResult.Failure("ticket is required");
        var cacheKey = $"external_login_{ticket}";
        if (!_cache.TryGetValue(cacheKey, out ExternalLoginTicket? extTicket) || extTicket == null)
            return TokenExchangeResult.Failure("Ticket invalide ou expiré");

        ApplicationUser? extUser;
        if (extTicket.ConsumedAt != null)
        {
            extUser = await _userManager.FindByIdAsync(extTicket.UserId.ToString());
            if (extUser == null || !extUser.IsActive)
                return TokenExchangeResult.Failure("Utilisateur introuvable ou inactif");
        }
        else
        {
            extTicket.ConsumedAt = DateTime.UtcNow;
            _cache.Set(cacheKey, extTicket, TimeSpan.FromSeconds(10));
            extUser = await _userManager.FindByIdAsync(extTicket.UserId.ToString());
        }
        if (extUser == null || !extUser.IsActive)
            return TokenExchangeResult.Failure("Utilisateur introuvable ou inactif");

        var extPrincipal = await _signInManager.CreateUserPrincipalAsync(extUser);
        var extRoles = await _userManager.GetRolesAsync(extUser);
        TokenPrincipalBuilder.SetUserClaims(extPrincipal, extUser, extRoles);
        TokenPrincipalBuilder.SetUserScopes(extPrincipal, request);
        TokenPrincipalBuilder.SetCommonDestinations(extPrincipal);
        return TokenExchangeResult.Success(extPrincipal);
    }

    private async Task<TokenExchangeResult> HandlePasswordAsync(HttpRequest request)
    {
        var username = request.Form["username"].FirstOrDefault();
        var password = request.Form["password"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return TokenExchangeResult.Failure("Username and password are required");

        var (loginUser, roles) = await _authService.ValidateCredentialsAsync(username, password);
        if (loginUser.TwoFactorEnabled)
        {
            var challengeToken = Guid.NewGuid().ToString("N");
            var cacheEntry = new TwoFactorChallenge(
                loginUser.Id, loginUser.Email!, DateTime.UtcNow.AddMinutes(5));
            _cache.Set($"2fa_challenge_{challengeToken}", cacheEntry, TimeSpan.FromMinutes(5));
            return TokenExchangeResult.TwoFactorRequired(challengeToken);
        }

        var loginPrincipal = await _signInManager.CreateUserPrincipalAsync(loginUser);
        TokenPrincipalBuilder.SetUserClaims(loginPrincipal, loginUser, roles);
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
}
