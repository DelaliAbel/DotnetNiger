using Asp.Versioning;
using DotnetNiger.Common.Auth.Requests;
using DotnetNiger.Common.Auth.Responses;
using DotnetNiger.Common.Email;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting("Auth")]
public class AccountController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly AccountService _accountService;
    private readonly SmtpOptions _smtp;

    public AccountController(AuthService authService, AccountService accountService, IOptions<SmtpOptions> smtp)
    {
        _authService = authService;
        _accountService = accountService;
        _smtp = smtp.Value;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserInfoResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var ua = Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
            var result = await _authService.LoginAsync(request.Email, request.Password, request.TenantId, request.RememberMe, ip, ua);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ErrorResponse(ex.Message));
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request)
    {
        var user = await _accountService.RegisterAsync(request.Email, request.Password, request.FirstName, request.LastName, request.TenantId);
        return Ok(new { message = "Compte créé. Un code de confirmation vous a été envoyé par email.", userId = user.Id, email = user.Email });
    }

    [HttpPost("confirm-email")]
    public async Task<ActionResult<object>> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        await _accountService.ConfirmEmailAsync(request.Email, request.Code);
        return Ok(new { message = "Email confirmé avec succès. Vous pouvez maintenant vous connecter." });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmailGet([FromQuery] string email, [FromQuery] string code)
    {
        await _accountService.ConfirmEmailAsync(email, code);
        return Redirect($"{_smtp.FrontendBaseUrl.TrimEnd('/')}/auth/login?emailConfirmed=true");
    }

    [HttpPost("resend-code")]
    public async Task<ActionResult<object>> ResendCode([FromBody] ForgotPasswordRequest request)
    {
        await _accountService.ResendConfirmationCodeAsync(request.Email);
        return Ok(new { message = "Un nouveau code de confirmation vous a été envoyé." });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _accountService.ForgotPasswordAsync(request.Email);
        return Ok(new { message = "Si le compte existe, un email de réinitialisation a été envoyé." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var error = await _accountService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        if (error != null)
            return BadRequest(new { message = error, code = error == "INVALID_EMAIL" ? "INVALID_EMAIL" : "RESET_FAILED" });
        return Ok(new { message = "Mot de passe réinitialisé avec succès." });
    }
}
