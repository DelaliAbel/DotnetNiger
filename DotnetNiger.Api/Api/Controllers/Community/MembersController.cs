using System.Threading;
using DotnetNiger.Api.Application.Interfaces;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
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
    public async Task<IActionResult> GetAll([FromQuery] string? query, [FromQuery] string? country, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await memberService.GetAllAsync(query, country, page, pageSize, ct);
        return Success(result);
    }

    /// <summary>Récupère les membres de l'équipe.</summary>
    [HttpGet("team")]
    public async Task<IActionResult> GetTeam(CancellationToken ct = default)
    {
        var members = await memberService.GetTeamMembersAsync(ct);
        return Success(members);
    }

    /// <summary>Récupère un membre par son identifiant.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var member = await memberService.GetByIdAsync(id, ct);
        if (member is null) return NotFound("Membre non trouvé");
        return Success(member);
    }

    /// <summary>Crée le profil de membre de l'utilisateur connecté.</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request, CancellationToken ct = default)
    {
        var userId = GetUserId();
        try
        {
            var member = await memberService.CreateProfileAsync(userId, request, ct);
            return CreatedAtAction(nameof(GetById), new { id = member.Id }, new { success = true, data = member, message = (string?)null });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Met à jour le profil de membre de l'utilisateur connecté.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberRequest request, CancellationToken ct = default)
    {
        try
        {
            var userId = GetUserId();
            var targetMember = await memberService.GetByIdAsync(id, ct);
            if (targetMember == null) return NotFound("Membre non trouvé");
            if (!IsAdmin() && targetMember.UserId != userId)
                return Failure("Vous ne pouvez modifier que votre propre profil.", 403);
            var member = await memberService.UpdateProfileAsync(targetMember.UserId, request, ct);
            return Success(member);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Membre non trouvé");
        }
    }

    /// <summary>Supprime le profil de membre.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var targetMember = await memberService.GetByIdAsync(id, ct);
        if (targetMember == null) return NotFound("Membre non trouvé");
        if (!IsAdmin() && targetMember.UserId != userId)
            return Failure("Vous ne pouvez supprimer que votre propre profil.", 403);
        var deleted = await memberService.DeleteProfileAsync(targetMember.UserId, ct);
        if (!deleted) return NotFound("Membre non trouvé");
        return Success<object?>(null, "Profil supprimé");
    }
}
