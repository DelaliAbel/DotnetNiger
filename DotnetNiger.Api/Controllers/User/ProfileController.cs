using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.DTOs.Requests;

namespace DotnetNiger.Api.Controllers.User;

/// <summary>
/// Controller du profil utilisateur.
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly AccountService _accountService;

    public ProfileController(IProfileService profileService, AccountService accountService)
    {
        _profileService = profileService;
        _accountService = accountService;
    }

    /// <summary>Récupère le profil complet de l'utilisateur connecté.</summary>
    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> GetProfile()
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        var profile = await _profileService.GetAsync(userId.Value);
        if (profile is null) return NotFound();
        return Ok(profile);
    }

    /// <summary>Met à jour le profil de l'utilisateur connecté.</summary>
    [HttpPut]
    public async Task<ActionResult<ProfileResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        var profile = await _profileService.UpdateAsync(userId.Value, request);
        if (profile is null) return NotFound();
        return Ok(profile);
    }

    /// <summary>Supprime le profil de l'utilisateur connecté.</summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteProfile()
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        await _accountService.DeleteProfileAsync(userId.Value);
        return NoContent();
    }

    /// <summary>Demande la suppression du compte (planifiée à 7 jours).</summary>
    [HttpPost("delete-request")]
    public async Task<IActionResult> RequestDeletion()
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();

        try
        {
            var request = await _accountService.RequestDeletionAsync(userId.Value);
            return Ok(new { Success = true, Message = "Votre compte sera supprimé dans 7 jours. Vous pouvez annuler cette demande.", ScheduledFor = request.ScheduledFor });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>Annule une demande de suppression en cours.</summary>
    [HttpPost("delete-request/cancel")]
    public async Task<IActionResult> CancelDeletion()
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();

        var cancelled = await _accountService.CancelDeletionAsync(userId.Value);
        if (!cancelled)
            return NotFound(new { Success = false, Message = "Aucune demande de suppression en cours." });
        return Ok(new { Success = true, Message = "Demande de suppression annulée." });
    }

    /// <summary>Initie le changement d'email.</summary>
    [HttpPost("change-email")]
    public async Task<ActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        await _accountService.ChangeEmailAsync(userId.Value, request.NewEmail);
        return Ok(new { message = "Un code de confirmation a été envoyé à votre nouvelle adresse email." });
    }

    /// <summary>Confirme le changement d'email.</summary>
    [HttpPost("confirm-change-email")]
    public async Task<ActionResult> ConfirmChangeEmail([FromBody] ConfirmChangeEmailRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        await _accountService.ConfirmChangeEmailAsync(userId.Value, request.Code);
        return Ok(new { message = "Adresse email modifiée avec succès." });
    }

    /// <summary>Change le mot de passe.</summary>
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        try
        {
            await _accountService.ChangePasswordAsync(userId.Value, request.CurrentPassword, request.NewPassword);
            return Ok(new { message = "Mot de passe changé avec succès." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>Extrait l'ID utilisateur du claim JWT.</summary>
    private Guid? GetUserIdFromClaims()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null) return null;
        return Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
