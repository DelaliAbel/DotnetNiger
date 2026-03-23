// Controleur API Community: MembersController
using Asp.Versioning;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/members")]
public class MembersController : ApiControllerBase
{
    // Endpoints pour la gestion des members.
    private readonly IMemberService _memberService;
    private readonly ILogger<MembersController> _logger;

    public MembersController(IMemberService memberService, ILogger<MembersController> logger)
    {
        _memberService = memberService;
        _logger = logger;
    }

    /// <summary>
    /// Récupérer la liste des members avec pagination
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Nombre de membres par page (par défaut 10, maximum 100)</param>
    /// <returns>Liste paginée des members</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllMembers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequestProblem("Parametres de pagination invalides");

        try
        {
            var members = await _memberService.GetAllMembersAsync(page, pageSize);
            var memberDtos = members.Select(MapToDto).ToList();
            return Success(memberDtos, meta: new { page, pageSize, total = memberDtos.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des members");
            throw;
        }
    }

    /// <summary>
    /// Récupérer la liste des members actifs
    /// </summary>
    /// <returns>Liste des members actifs</returns>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveMembers()
    {
        try
        {
            var members = await _memberService.GetActiveMembersAsync();
            var memberDtos = members.Select(MapToDto).ToList();
            return Success(memberDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des members actifs");
            throw;
        }
    }

    /// <summary>
    /// Récupérer un member par ID
    /// </summary>
    /// <param name="id">ID du member</param>
    /// <returns>Détails du member</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMemberById(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequestProblem("L'ID du member est requis");

        try
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
                return NotFoundProblem($"Member avec l'ID {id} non trouvé");

            return Success(MapToDto(member));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du member {MemberId}", id);
            throw;
        }
    }

    /// <summary>
    /// Créer un nouveau member
    /// </summary>
    /// <param name="request">Données du member à créer</param>
    /// <returns>Member créé</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMember([FromBody] CreateMemberRequest request)
    {
        if (request == null)
            return BadRequestProblem("Les données du member sont requises");

        try
        {
            var memberEntity = new Member
            {
                UserId = request.UserId,
                Name = request.Name,
                Position = request.Position,
                BioOverride = request.BioOverride ?? string.Empty,
                Order = request.Order,
                IsPublic = request.IsPublic,
                IsActive = request.IsActive,
                RoleDescription = request.RoleDescription ?? string.Empty,
                JoinedAt = DateTime.UtcNow
            };
            var createdMember = await _memberService.CreateMemberAsync(memberEntity);
            return CreatedSuccess(nameof(GetMemberById), new { id = createdMember.Id }, MapToDto(createdMember), "Member créé avec succès");
        }
        catch (ArgumentException ex)
        {
            return BadRequestProblem(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du member");
            throw;
        }
    }

    /// <summary>
    /// Mettre à jour un member existant
    /// </summary>
    /// <param name="id">ID du member à mettre à jour</param>
    /// <param name="request">Données du member à mettre à jour</param>
    /// <returns>Member mis à jour</returns>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] UpdateMemberRequest request)
    {
        if (id == Guid.Empty)
            return BadRequestProblem("L'ID du member est requis");

        if (request == null)
            return BadRequestProblem("Les données du member sont requises");

        try
        {
            var existingMember = await _memberService.GetMemberByIdAsync(id);
            if (existingMember == null)
                return NotFoundProblem($"Member avec l'ID {id} non trouvé");

            if (!string.IsNullOrEmpty(request.Name))
                existingMember.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Position))
                existingMember.Position = request.Position;
            if (request.BioOverride != null)
                existingMember.BioOverride = request.BioOverride;
            if (request.Order.HasValue)
                existingMember.Order = request.Order.Value;
            if (request.IsPublic.HasValue)
                existingMember.IsPublic = request.IsPublic.Value;
            if (request.IsActive.HasValue)
                existingMember.IsActive = request.IsActive.Value;
            if (request.RoleDescription != null)
                existingMember.RoleDescription = request.RoleDescription;

            var updatedMember = await _memberService.UpdateMemberAsync(id, existingMember);
            return Success(MapToDto(updatedMember), "Member mis à jour avec succès");
        }
        catch (ArgumentException ex)
        {
            return BadRequestProblem(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du member {MemberId}", id);
            throw;
        }
    }

    /// <summary>
    /// Supprimer un member
    /// </summary>
    /// <param name="id">ID du member à supprimer</param>
    /// <returns>Résultat de la suppression</returns>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequestProblem("L'ID du member est requis");

        try
        {
            var deleted = await _memberService.DeleteMemberAsync(id);
            if (!deleted)
                return NotFoundProblem($"Member avec l'ID {id} non trouvé");

            return SuccessMessage("Member supprimé avec succès");
        }
        catch (ArgumentException ex)
        {
            return BadRequestProblem(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du member {MemberId}", id);
            throw;
        }
    }

    /// <summary>Maps a Member entity to a TeamMemberDto.</summary>
    private static TeamMemberDto MapToDto(Member member)
    {
        return new TeamMemberDto
        {
            Id = member.Id,
            UserId = member.UserId,
            Username = string.Empty, // Will be populated from Identity service if needed
            FullName = member.Name,
            AvatarUrl = string.Empty, // Will be populated from Identity service if needed
            Position = member.Position,
            BioOverride = member.BioOverride ?? string.Empty,
            RoleDescription = member.RoleDescription ?? string.Empty,
            SocialLinks = new(),
            Skills = new()
        };
    }
}
