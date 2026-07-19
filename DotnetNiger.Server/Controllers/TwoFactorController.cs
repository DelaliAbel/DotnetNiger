using Asp.Versioning;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Server.AspNetCore;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting("Auth")]
public class TwoFactorController : ControllerBase
{
    private readonly TwoFactorService _twoFactorService;

    public TwoFactorController(TwoFactorService twoFactorService) => _twoFactorService = twoFactorService;

    [AllowAnonymous]
    [HttpPost("verify-2fa")]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] Verify2faRequest request)
    {
        var result = await _twoFactorService.VerifyAsync(request.Code, request.ChallengeToken, Request);
        if (result.Principal != null)
            return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (result.RateLimited)
            return BadRequest(new { error = "Trop de tentatives. Réessayez dans une minute." });
        return BadRequest(new ErrorResponse(result.Error ?? "Vérification échouée"));
    }

    [AllowAnonymous]
    [HttpPost("verify-2fa-recovery")]
    public async Task<IActionResult> VerifyTwoFactorRecovery([FromBody] TwoFactorRecoveryCodeRequest request)
    {
        var result = await _twoFactorService.VerifyRecoveryAsync(request.RecoveryCode, request.ChallengeToken, Request);
        if (result.Principal != null)
            return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (result.RateLimited)
            return BadRequest(new { error = "Trop de tentatives. Réessayez dans une minute." });
        return BadRequest(new ErrorResponse(result.Error ?? "Code de récupération invalide"));
    }
}
