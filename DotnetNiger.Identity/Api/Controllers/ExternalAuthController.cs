using Asp.Versioning;

using DotnetNiger.Common.Auth.Responses;
using DotnetNiger.Identity.Application.Services;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting("Auth")]
public class ExternalAuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IPermissionService _permissionService;
    private readonly IConfiguration _config;

    public ExternalAuthController(
        AuthService authService,
        SignInManager<ApplicationUser> signInManager,
        IPermissionService permissionService,
        IConfiguration config)
    {
        _authService = authService;
        _signInManager = signInManager;
        _permissionService = permissionService;
        _config = config;
    }

    /// <summary>Déclenche une authentification externe via le fournisseur spécifié.</summary>
    /// <param name="provider">Nom du fournisseur externe (Google, Facebook, etc.).</param>
    /// <param name="returnUrl">URL de retour après authentification.</param>
    /// <param name="target">Cible du rappel (backend ou frontend).</param>
    [HttpGet("external-login")]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string? returnUrl, [FromQuery] string? target = null)
    {
        var callbackAction = target == "frontend" ? nameof(ExternalCallbackFrontend) : nameof(ExternalCallback);
        var redirectUrl = Url.Action(callbackAction, new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    /// <summary>Point de rappel pour l'authentification externe côté backend.</summary>
    /// <param name="returnUrl">URL de retour après authentification.</param>
    /// <param name="rememberMe">Conserver la session active.</param>
    [HttpGet("external-callback")]
    public async Task<ActionResult<UserInfoResponse>> ExternalCallback(
        [FromQuery] string? returnUrl = null, [FromQuery] bool rememberMe = false)
    {
        var (user, roles) = await _authService.HandleExternalLoginAsync("external");
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
        return Ok(new UserInfoResponse(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.AvatarUrl, user.TenantId, user.IsActive,
            roles, permissions, rememberMe));
    }

    /// <summary>Point de rappel pour l'authentification externe côté frontend.</summary>
    /// <param name="returnUrl">URL de retour après authentification.</param>
    [HttpGet("external-callback-frontend")]
    public async Task<IActionResult> ExternalCallbackFrontend([FromQuery] string? returnUrl = null)
    {
        var frontendBaseUrl = _config["FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5201";
        returnUrl = $"{frontendBaseUrl}/auth/external-callback";
        try
        {
            var redirectUrl = await _authService.HandleExternalCallbackFrontendAsync(returnUrl);
            return Redirect(redirectUrl);
        }
        catch (InvalidOperationException ex)
        {
            return Redirect($"{returnUrl}?error={Uri.EscapeDataString(ex.Message)}");
        }
    }
}
