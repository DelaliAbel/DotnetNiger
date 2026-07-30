using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Api.Application.DTOs.Requests;

namespace DotnetNiger.Api.Controllers.User;

/// <summary>
/// Controller du profil utilisateur.
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : BaseController
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
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        var profile = await _profileService.GetAsync(userId.Value);
        if (profile is null) return NotFound();
        return Success(profile);
    }

    /// <summary>Met à jour le profil de l'utilisateur connecté.</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        var profile = await _profileService.UpdateAsync(userId.Value, request);
        if (profile is null) return NotFound();
        return Success(profile);
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
            return Success(new { ScheduledFor = request.ScheduledFor }, "Votre compte sera supprimé dans 7 jours. Vous pouvez annuler cette demande.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
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
            return NotFound("Aucune demande de suppression en cours.");
        return Success<object?>(null, "Demande de suppression annulée.");
    }

    /// <summary>Initie le changement d'email.</summary>
    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        await _accountService.ChangeEmailAsync(userId.Value, request.NewEmail);
        return Success<object?>(null, "Un code de confirmation a été envoyé à votre nouvelle adresse email.");
    }

    /// <summary>Confirme le changement d'email.</summary>
    [HttpPost("confirm-change-email")]
    public async Task<IActionResult> ConfirmChangeEmail([FromBody] ConfirmChangeEmailRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        await _accountService.ConfirmChangeEmailAsync(userId.Value, request.Code);
        return Success<object?>(null, "Adresse email modifiée avec succès.");
    }

    /// <summary>Change le mot de passe.</summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        try
        {
            await _accountService.ChangePasswordAsync(userId.Value, request.CurrentPassword, request.NewPassword);
            return Success<object?>(null, "Mot de passe changé avec succès.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Récupère les liens sociaux du profil.</summary>
    [HttpGet("social-links")]
    public async Task<IActionResult> GetSocialLinks()
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        try
        {
            var links = await _profileService.GetSocialLinksAsync(userId.Value);
            return Success(links);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Profil membre non trouvé.");
        }
    }

    /// <summary>Ajoute un lien social au profil.</summary>
    [HttpPost("social-links")]
    public async Task<IActionResult> AddSocialLink([FromBody] AddSocialLinkRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        try
        {
            var link = await _profileService.AddSocialLinkAsync(userId.Value, request);
            return Success(link);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Profil membre non trouvé. Créez d'abord votre profil.");
        }
    }

    /// <summary>Supprime un lien social du profil.</summary>
    [HttpDelete("social-links/{linkId:guid}")]
    public async Task<IActionResult> DeleteSocialLink(Guid linkId)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null) return Unauthorized();
        var deleted = await _profileService.DeleteSocialLinkAsync(userId.Value, linkId);
        if (!deleted) return NotFound("Lien social non trouvé.");
        return NoContent();
    }

    /// <summary>Extrait l'ID utilisateur du claim JWT.</summary>
    private Guid? GetUserIdFromClaims()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null) return null;
        return Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
