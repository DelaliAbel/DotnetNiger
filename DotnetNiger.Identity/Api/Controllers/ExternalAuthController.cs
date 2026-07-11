using Asp.Versioning;
using DotnetNiger.Common.Email;
using DotnetNiger.Common.Auth.Responses;
using DotnetNiger.Identity.Application.Services;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

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
    private readonly SmtpOptions _smtp;

    public ExternalAuthController(
        AuthService authService,
        SignInManager<ApplicationUser> signInManager,
        IPermissionService permissionService,
        IOptions<SmtpOptions> smtp)
    {
        _authService = authService;
        _signInManager = signInManager;
        _permissionService = permissionService;
        _smtp = smtp.Value;
    }

    [HttpGet("external-login")]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string? returnUrl, [FromQuery] string? target = null)
    {
        var callbackAction = target == "frontend" ? nameof(ExternalCallbackFrontend) : nameof(ExternalCallback);
        var redirectUrl = Url.Action(callbackAction, new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

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

    [HttpGet("external-callback-frontend")]
    public async Task<IActionResult> ExternalCallbackFrontend([FromQuery] string? returnUrl = null)
    {
        var frontendBaseUrl = _smtp.FrontendBaseUrl.TrimEnd('/');
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
