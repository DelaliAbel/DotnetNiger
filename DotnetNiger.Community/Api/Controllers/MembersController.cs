using Asp.Versioning;
using MediatR;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Api.Filters;
using DotnetNiger.Community.Application.Features.Members.Commands;
using DotnetNiger.Community.Application.Features.Members.Queries;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/members")]
public class MembersController : ApiControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<MembersController> _logger;
    private readonly IConfiguration _configuration;

    public MembersController(
        ISender sender,
        IConfiguration configuration,
        ILogger<MembersController> logger)
    {
        _sender = sender;
        _configuration = configuration;
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
            var members = await _sender.Send(new GetAllMembersQuery(page, pageSize));
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
            var members = await _sender.Send(new GetActiveMembersQuery());
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
            var member = await _sender.Send(new GetMemberByIdQuery(id));
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
    public async Task<IActionResult> CreateMember([FromBody] CreateTeamMemberRequest request)
    {
        if (request == null)
            return BadRequestProblem("Les données du member sont requises");

        try
        {
            var createdMember = await _sender.Send(new CreateMemberCommand(
                request.UserId,
                request.Name,
                request.Position,
                request.BioOverride,
                request.Order,
                request.IsPublic,
                request.RoleDescription));
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

    [HttpPost("internal/provision")]
    [AllowAnonymous]
    public async Task<IActionResult> ProvisionPendingMember([FromBody] ProvisionMemberRequest request)
    {
        var expectedKey = _configuration["Integration:ProvisioningApiKey"];
        var providedKey = HttpContext.Request.Headers["X-Internal-Key"].ToString();
        if (string.IsNullOrWhiteSpace(expectedKey) || !string.Equals(expectedKey, providedKey, StringComparison.Ordinal))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Invalid integration key.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var createdMember = await _sender.Send(new CreateMemberCommand(
            request.UserId,
            string.IsNullOrWhiteSpace(request.FullName) ? "Member" : request.FullName,
            "Volunteer",
            string.Empty,
            0,
            false,
            "Auto-provisioned pending member"));
        return Success(MapToDto(createdMember), "Pending member provisioned.");
    }

    [HttpPatch("{id}/approve")]
    [AuthorizeFilter("Admin", "SuperAdmin")]
    public async Task<IActionResult> ApproveMember(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequestProblem("L'ID du member est requis");

        var reviewerUserId = ResolveReviewerUserId();
        var approvalResult = await _sender.Send(new ApproveMemberCommand(id, reviewerUserId));

        if (!approvalResult.RoleAssigned)
        {
            _logger.LogWarning("Member {MemberId} approved but role assignment failed for user {UserId}", approvalResult.Member.Id, approvalResult.Member.UserId);
        }

        return Success(MapToDto(approvalResult.Member), approvalResult.RoleAssigned
            ? "Member approuve et role attribue."
            : "Member approuve, mais attribution du role a echoue.");
    }

    [HttpPatch("{id}/reject")]
    [AuthorizeFilter("Admin", "SuperAdmin")]
    public async Task<IActionResult> RejectMember(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequestProblem("L'ID du member est requis");

        var reviewerUserId = ResolveReviewerUserId();
        var rejected = await _sender.Send(new RejectMemberCommand(id, reviewerUserId));
        return Success(MapToDto(rejected), "Member rejete.");
    }

    private Guid ResolveReviewerUserId()
    {
        if (Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var reviewerId))
        {
            return reviewerId;
        }

        var headerValue = HttpContext.Request.Headers["X-Admin-UserId"].ToString();
        if (Guid.TryParse(headerValue, out reviewerId))
        {
            return reviewerId;
        }

        throw new UnauthorizedAccessException("Reviewer user id is required in JWT or X-Admin-UserId header.");
    }

    /// <summary>
    /// Mettre à jour un member existant
    /// </summary>
    /// <param name="id">ID du member à mettre à jour</param>
    /// <param name="request">Données du member à mettre à jour</param>
    /// <returns>Member mis à jour</returns>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] UpdateTeamMemberRequest request)
    {
        if (id == Guid.Empty)
            return BadRequestProblem("L'ID du member est requis");

        if (request == null)
            return BadRequestProblem("Les données du member sont requises");

        try
        {
            var updatedMember = await _sender.Send(new UpdateMemberCommand(
                id,
                request.Name,
                request.Position,
                request.BioOverride,
                request.Order,
                request.IsPublic,
                request.IsActive,
                request.RoleDescription));

            if (updatedMember == null)
                return NotFoundProblem($"Member avec l'ID {id} non trouvé");

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
            var deleted = await _sender.Send(new DeleteMemberCommand(id));
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

    /// <summary>Maps a TeamMember entity to a TeamMemberResponse.</summary>
    private static TeamMemberResponse MapToDto(TeamMember member)
    {
        return new TeamMemberResponse
        {
            Id = member.Id,
            UserId = member.UserId,
            Username = string.Empty, // Will be populated from Identity service if needed
            FullName = member.Name,
            AvatarUrl = string.Empty, // Will be populated from Identity service if needed
            Position = member.Position,
            MembershipStatus = member.MembershipStatus.ToString(),
            BioOverride = member.BioOverride ?? string.Empty,
            RoleDescription = member.RoleDescription ?? string.Empty,
            SocialLinks = new(),
            Skills = new()
        };
    }
}

public sealed class ProvisionMemberRequest
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
}
