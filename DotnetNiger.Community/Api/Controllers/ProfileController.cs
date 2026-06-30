using Asp.Versioning;
using System.Security.Claims;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion du profil utilisateur (informations, liens sociaux, certificats).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ProfileController(IProfileService profileService) : BaseController
{
    /// <summary>Récupère le profil complet de l'utilisateur connecté.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();
        var profile = await profileService.GetAsync(userId);
        if (profile is null) return NotFound(new { Success = false, Message = Messages.Profile.NotFound });

        profile.Email = User.FindFirstValue(ClaimTypes.Email) ?? "";
        profile.Username = User.FindFirstValue(ClaimTypes.Name) ?? profile.Email;
        profile.Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct().ToList();

        return Ok(new { Success = true, Data = profile });
    }

    /// <summary>Met à jour le profil de l'utilisateur connecté.</summary>
    /// <param name="request">Nouvelles informations du profil.</param>
    [HttpPut("me")]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        var profile = await profileService.UpdateAsync(userId, request);
        if (profile is null) return NotFound(new { Success = false, Message = Messages.Profile.NotFound });

        profile.Email = User.FindFirstValue(ClaimTypes.Email) ?? "";
        profile.Username = User.FindFirstValue(ClaimTypes.Name) ?? profile.Email;
        profile.Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct().ToList();

        return Ok(new { Success = true, Data = profile });
    }

    /// <summary>Récupère les liens sociaux de l'utilisateur connecté.</summary>
    [HttpGet("social-links")]
    public async Task<IActionResult> GetSocialLinks()
    {
        var userId = GetUserId();
        var profile = await profileService.GetAsync(userId);
        return Ok(new { Success = true, Data = profile?.SocialLinks ?? [] });
    }

    /// <summary>Ajoute un lien social au profil de l'utilisateur.</summary>
    /// <param name="request">Plateforme et URL du lien.</param>
    [HttpPost("social-links")]
    public async Task<IActionResult> AddSocialLink([FromBody] AddSocialLinkRequest request)
    {
        var userId = GetUserId();
        var link = await profileService.AddSocialLinkAsync(userId, request);
        return Ok(new { Success = true, Data = link });
    }

    /// <summary>Supprime un lien social du profil.</summary>
    /// <param name="id">Identifiant du lien social.</param>
    [HttpDelete("social-links/{id:guid}")]
    public async Task<IActionResult> DeleteSocialLink(Guid id)
    {
        var userId = GetUserId();
        var deleted = await profileService.DeleteSocialLinkAsync(userId, id);
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Profile.SocialLinkNotFound });
        return Ok(new { Success = true, Message = Messages.Profile.SocialLinkDeleted });
    }

    /// <summary>Soumet un certificat pour validation par l'équipe.</summary>
    /// <param name="request">URL et type du certificat.</param>
    [HttpPost("certificates")]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitCertificate([FromBody] CertificateSubmissionRequest request)
    {
        try
        {
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                userId = request.UserId;
            }

            if (userId == Guid.Empty)
                return Unauthorized(new { Success = false, Message = Messages.User.InvalidIdentity });

            var cert = await profileService.SubmitCertificateAsync(userId, request);
            return Ok(new { Success = true, Data = cert });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
}


