using DotnetNiger.Api.Services;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Community;

/// <summary>Contrôleur de gestion des membres de la communauté.</summary>
[ApiController]
[Route("api/members")]
public class MembersController(IMemberDirectoryService memberService) : BaseController
{
    /// <summary>Récupère la liste paginée des membres avec filtres optionnels.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? query, [FromQuery] string? country, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await memberService.GetAllAsync(query, country, page, pageSize);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>Récupère les membres de l'équipe.</summary>
    [HttpGet("team")]
    public async Task<IActionResult> GetTeam()
    {
        var members = await memberService.GetTeamMembersAsync();
        return Ok(new { Success = true, Data = members });
    }

    /// <summary>Récupère un membre par son identifiant.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await memberService.GetByIdAsync(id);
        if (member is null) return NotFound(new { Success = false, Message = "Membre non trouvé" });
        return Ok(new { Success = true, Data = member });
    }

    /// <summary>Crée le profil de membre de l'utilisateur connecté.</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
    {
        var userId = GetUserId();
        try
        {
            var member = await memberService.CreateProfileAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = member.Id }, new { Success = true, Data = member });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>Met à jour le profil de membre de l'utilisateur connecté.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberRequest request)
    {
        try
        {
            var member = await memberService.UpdateProfileAsync(GetUserId(), request);
            return Ok(new { Success = true, Data = member });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Success = false, Message = "Membre non trouvé" });
        }
    }

    /// <summary>Supprime le profil de membre de l'utilisateur connecté.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await memberService.DeleteProfileAsync(GetUserId());
        if (!deleted) return NotFound(new { Success = false, Message = "Membre non trouvé" });
        return Ok(new { Success = true, Message = "Profil supprimé" });
    }
}
