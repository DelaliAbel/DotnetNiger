// Controleur API Identity: AuthController
using Asp.Versioning;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
// Endpoints publics pour authentification et reinitialisation.
public class AuthController : ApiControllerBase
{
    // Endpoints publics pour l'authentification.
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IAuthService authService, ITokenService tokenService, IWebHostEnvironment environment)
    {
        _authService = authService;
        _tokenService = tokenService;
        _environment = environment;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Success(result, "Registration successful.");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Success(result, "Login successful.");
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var token = await _authService.RequestPasswordResetAsync(request);
        if (_environment.IsDevelopment())
        {
            return Success(new { token }, "Reset token generated.");
        }

        return SuccessMessage("If the email exists, a reset link was sent.");
    }

    [HttpPost("request-email-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestEmailVerification([FromBody] RequestEmailVerificationRequest request)
    {
        var token = await _authService.RequestEmailVerificationAsync(request);
        if (_environment.IsDevelopment())
        {
            return Success(new { token }, "Verification token generated.");
        }

        return SuccessMessage("If the email exists, a verification email was sent.");
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return SuccessMessage("Password reset successful.");
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        await _authService.VerifyEmailAsync(request);
        return SuccessMessage("Email verified.");
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _tokenService.RefreshAsync(request);
        return Success(result, "Token refreshed.");
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var userId = RequireAuthenticatedUserId();
        await _tokenService.LogoutAsync(userId, request);
        return SuccessMessage("Logout successful.");
    }
}
