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
    private readonly IClientRateLimiter _rateLimiter;
    private readonly IFeatureToggleService _featureToggleService;
    private readonly IWebHostEnvironment _environment;
    private const int MaxLoginAttempts = 5;
    private const int RateLimitWindowMinutes = 15;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService,
        IClientRateLimiter rateLimiter,
        IFeatureToggleService featureToggleService,
        IWebHostEnvironment environment)
    {
        _authService = authService;
        _tokenService = tokenService;
        _rateLimiter = rateLimiter;
        _featureToggleService = featureToggleService;
        _environment = environment;
    }

    /// <summary>
    /// Register new user (rate limited: 3 attempts per 15 min per IP)
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!_featureToggleService.IsRegistrationEnabled())
            return StatusCode(503, new ProblemDetails { Title = "Feature disabled", Detail = "Registration is currently disabled.", Status = 503 });

        if (!TryConsumeRateLimit("register", 3, out var retryAfter))
            return StatusCode(429, new { error = $"Too many registration attempts. Retry in {(int)Math.Ceiling((retryAfter ?? TimeSpan.Zero).TotalMinutes)} minutes." });

        var result = await _authService.RegisterAsync(request);
        return Success(result, "Registration successful.");
    }

    /// <summary>
    /// Login endpoint with brute-force protection (5 attempts per 15 min per IP)
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!_featureToggleService.IsLoginEnabled())
            return StatusCode(503, new ProblemDetails { Title = "Feature disabled", Detail = "Login is currently disabled.", Status = 503 });

        if (!TryConsumeRateLimit("login", MaxLoginAttempts, out var retryAfter))
            return StatusCode(429, new { error = $"Too many login attempts. Retry in {(int)Math.Ceiling((retryAfter ?? TimeSpan.Zero).TotalMinutes)} minutes." });

        try
        {
            var result = await _authService.LoginAsync(request);
            return Success(result, "Login successful.");
        }
        catch
        {
            // Failed login keeps counter until window expires
            throw;
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!_featureToggleService.IsPasswordResetEnabled())
            return StatusCode(503, new ProblemDetails { Title = "Feature disabled", Detail = "Password reset is currently disabled.", Status = 503 });

        if (!TryConsumeRateLimit("forgot-password", 3, out var retryAfter))
            return StatusCode(429, new { error = $"Too many password reset attempts. Retry in {(int)Math.Ceiling((retryAfter ?? TimeSpan.Zero).TotalMinutes)} minutes." });

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
        if (!_featureToggleService.IsEmailVerificationEnabled())
            return StatusCode(503, new ProblemDetails { Title = "Feature disabled", Detail = "Email verification is currently disabled.", Status = 503 });

        if (!TryConsumeRateLimit("request-email-verification", 3, out var retryAfter))
            return StatusCode(429, new { error = $"Too many verification requests. Retry in {(int)Math.Ceiling((retryAfter ?? TimeSpan.Zero).TotalMinutes)} minutes." });

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
        if (!_featureToggleService.IsPasswordResetEnabled())
            return StatusCode(503, new ProblemDetails { Title = "Feature disabled", Detail = "Password reset is currently disabled.", Status = 503 });

        await _authService.ResetPasswordAsync(request);
        return SuccessMessage("Password reset successful.");
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        if (!_featureToggleService.IsEmailVerificationEnabled())
            return StatusCode(503, new ProblemDetails { Title = "Feature disabled", Detail = "Email verification is currently disabled.", Status = 503 });

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

    /// <summary>
    /// Helper: Get client IP address from HTTP context
    /// </summary>
    private string GetClientIpAddress()
    {
        if (HttpContext?.Connection?.RemoteIpAddress != null)
        {
            return HttpContext.Connection.RemoteIpAddress.ToString();
        }

        var forwardedFor = HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(",").First().Trim();
        }

        return "unknown";
    }

    private bool TryConsumeRateLimit(string action, int limit, out TimeSpan? retryAfter)
    {
        var clientId = GetClientIpAddress();
        return _rateLimiter.TryConsume(action, clientId, limit, TimeSpan.FromMinutes(RateLimitWindowMinutes), out retryAfter);
    }
}
