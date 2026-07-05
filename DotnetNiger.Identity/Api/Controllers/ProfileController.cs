using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static OpenIddict.Abstractions.OpenIddictConstants;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Common.Auth.Requests;
using DotnetNiger.Common.Auth.Responses;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Services;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly TwoFactorService _twoFactorService;
    private readonly AccountService _accountService;

    public ProfileController(
        AuthService authService,
        TwoFactorService twoFactorService,
        AccountService accountService)
    {
        _authService = authService;
        _twoFactorService = twoFactorService;
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        return Ok(await _accountService.GetProfileAsync(Guid.Parse(userId)));
    }

    [HttpPut]
    public async Task<ActionResult<UserProfileResponse>> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        return Ok(await _accountService.UpdateProfileAsync(Guid.Parse(userId), request));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProfile()
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        await _accountService.DeleteProfileAsync(Guid.Parse(userId));
        return NoContent();
    }

    [HttpGet("two-factor/status")]
    public async Task<ActionResult<TwoFactorStatusResponse>> GetTwoFactorStatus()
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        return Ok(await _twoFactorService.GetStatusAsync(Guid.Parse(userId)));
    }

    [HttpPost("two-factor/setup")]
    public async Task<ActionResult<TwoFactorSetupResponse>> SetupTwoFactor()
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        var (sharedKey, authenticatorUri) = await _twoFactorService.GetSetupAsync(Guid.Parse(userId));
        return Ok(new TwoFactorSetupResponse(sharedKey, authenticatorUri, false));
    }

    [HttpPost("two-factor/enable")]
    public async Task<ActionResult> EnableTwoFactor([FromBody] Enable2faRequest request)
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        try
        {
            var (success, recoveryCodes) = await _twoFactorService.EnableAsync(Guid.Parse(userId), request.Code);
            if (!success) return BadRequest(new ErrorResponse("Impossible d'activer la double authentification"));
            return Ok(new RecoveryCodesResponse(recoveryCodes, recoveryCodes.Length));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpPost("two-factor/disable")]
    public async Task<ActionResult> DisableTwoFactor([FromBody] Disable2faRequest request)
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        var (success, error) = await _twoFactorService.DisableAsync(Guid.Parse(userId), request.Code);
        if (!success) return BadRequest(new ErrorResponse(error ?? "Impossible de désactiver la double authentification"));
        return Ok(new { message = "Double authentification désactivée" });
    }

    [HttpPost("two-factor/recovery-codes")]
    public async Task<ActionResult<RecoveryCodesResponse>> GenerateRecoveryCodes()
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        var codes = await _twoFactorService.GenerateRecoveryCodesAsync(Guid.Parse(userId));
        return Ok(new RecoveryCodesResponse(codes, codes.Length));
    }

    [HttpGet("login-history")]
    public async Task<ActionResult> GetLoginHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        var result = await _authService.GetLoginHistoryAsync(Guid.Parse(userId), Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
        return Ok(result);
    }

    [HttpPost("change-email")]
    public async Task<ActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        await _accountService.ChangeEmailAsync(Guid.Parse(userId), request.NewEmail);
        return Ok(new { message = "Un code de confirmation a été envoyé à votre nouvelle adresse email." });
    }

    [HttpPost("confirm-change-email")]
    public async Task<ActionResult> ConfirmChangeEmail([FromBody] ConfirmChangeEmailRequest request)
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        await _accountService.ConfirmChangeEmailAsync(Guid.Parse(userId), request.Code);
        return Ok(new { message = "Adresse email modifiée avec succès." });
    }

    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();
        try
        {
            await _accountService.ChangePasswordAsync(Guid.Parse(userId), request.CurrentPassword, request.NewPassword);
            return Ok(new { message = "Mot de passe changé avec succès." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }
}
