using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static OpenIddict.Abstractions.OpenIddictConstants;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.DTOs.Requests;

namespace DotnetNiger.Api.Controllers.User;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly AccountService _accountService;

    public ProfileController(AuthService authService, AccountService accountService)
    {
        _authService = authService;
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

    [HttpPost("delete-request")]
    public async Task<IActionResult> RequestDeletion()
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();

        try
        {
            var request = await _accountService.RequestDeletionAsync(Guid.Parse(userId));
            return Ok(new { Success = true, Message = "Votre compte sera supprimé dans 7 jours. Vous pouvez annuler cette demande.", ScheduledFor = request.ScheduledFor });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("delete-request/cancel")]
    public async Task<IActionResult> CancelDeletion()
    {
        var userId = User.FindFirst(Claims.Subject)?.Value;
        if (userId == null) return Unauthorized();

        var cancelled = await _accountService.CancelDeletionAsync(Guid.Parse(userId));
        if (!cancelled)
            return NotFound(new { Success = false, Message = "Aucune demande de suppression en cours." });
        return Ok(new { Success = true, Message = "Demande de suppression annulée." });
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
