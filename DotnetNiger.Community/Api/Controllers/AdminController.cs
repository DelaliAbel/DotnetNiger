using Asp.Versioning;
using DotnetNiger.Common.Constants;
using DotnetNiger.Community.Application.DTOs.Requests;
using AssignRoleRequest = DotnetNiger.Common.DTOs.Requests.AssignRoleRequest;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Application.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Points d'accès pour l'administration de la plateforme.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
public class AdminController(
    IAdminService adminService,
    IEventQueryService eventQuery,
    IEventModerationService eventModeration,
    ICertificateService certificateService,
    IPostQueryService postQuery,
    ICommentService commentService) : ControllerBase
{
    /// <summary>Retourne les statistiques du tableau de bord administration.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var stats = await adminService.GetDashboardAsync();
        return Ok(new { Success = true, Data = stats });
    }

    /// <summary>Retourne la liste des événements avec filtrage par statut et pagination.</summary>
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(new
        {
            Success = true,
            Data = status == "pending"
                ? await eventQuery.GetPendingEventsAsync(page, pageSize)
                : await eventQuery.GetAllAsync(null, null, null, null, null, null, null, null, page, pageSize)
        });
    }

    /// <summary>Publie un événement par son identifiant.</summary>
    [HttpPatch("events/{id:guid}/publish")]
    public async Task<IActionResult> PublishEvent(Guid id)
    {
        var ev = await eventModeration.PublishAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    /// <summary>Dépublie un événement par son identifiant.</summary>
    [HttpPatch("events/{id:guid}/unpublish")]
    public async Task<IActionResult> UnpublishEvent(Guid id)
    {
        var ev = await eventModeration.UnpublishAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    /// <summary>Approuve un événement en attente de modération.</summary>
    [HttpPatch("events/{id:guid}/approve")]
    public async Task<IActionResult> ApproveEvent(Guid id)
    {
        var ev = await eventModeration.ApproveAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    /// <summary>Rejette un événement avec une raison donnée.</summary>
    [HttpPatch("events/{id:guid}/reject")]
    public async Task<IActionResult> RejectEvent(Guid id, [FromQuery] string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return BadRequest(new { Success = false, Message = Messages.Certificate.RejectReasonRequired });
        var ev = await eventModeration.RejectAsync(id, reason);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Message = Messages.Event.Rejected, Data = ev });
    }

    /// <summary>Retourne la liste de tous les utilisateurs.</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        return Ok(new { Success = true, Data = await adminService.GetUsersAsync() });
    }

    /// <summary>Retourne un utilisateur par son identifiant.</summary>
    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await adminService.GetUserAsync(id);
        if (user is null) return NotFound(new { Success = false, Message = Messages.User.NotFound });
        return Ok(new { Success = true, Data = user });
    }

    /// <summary>Met à jour le statut actif/inactif d'un utilisateur.</summary>
    [HttpPatch("users/{id:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        var updated = await adminService.UpdateUserStatusAsync(id, request.IsActive);
        if (!updated) return NotFound(new { Success = false, Message = Messages.User.NotFound });
        return Ok(new { Success = true, Message = Messages.User.StatusUpdated });
    }

    /// <summary>Met à jour l'appartenance à l'équipe et la position d'un utilisateur.</summary>
    [HttpPatch("users/{id:guid}/team")]
    public async Task<IActionResult> UpdateUserTeam(Guid id, [FromBody] UpdateUserTeamRequest request)
    {
        var updated = await adminService.UpdateUserTeamAsync(id, request.IsTeamMember, request.Position);
        if (!updated) return NotFound(new { Success = false, Message = Messages.User.NotFound });
        return Ok(new { Success = true, Message = Messages.User.TeamUpdated });
    }

    /// <summary>Crée un nouvel utilisateur administrateur.</summary>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserRequest request)
    {
        var user = await adminService.CreateUserAsync(request);
        if (user is null) return BadRequest(new { Success = false, Message = Messages.User.CreateFailed });
        return Ok(new { Success = true, Data = user });
    }

    /// <summary>Supprime un utilisateur par son identifiant.</summary>
    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var deleted = await adminService.DeleteUserAsync(id);
        if (!deleted) return NotFound(new { Success = false, Message = Messages.User.NotFound });
        return Ok(new { Success = true, Message = Messages.User.Deleted });
    }

    /// <summary>Assigne un rôle à un utilisateur.</summary>
    [HttpPost("users/{userId:guid}/roles")]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, [FromBody] AssignRoleRequest request)
    {
        var assigned = await adminService.AssignRoleToUserAsync(userId, request.RoleName);
        if (!assigned) return BadRequest(new { Success = false, Message = Messages.User.RoleFailed });
        return Ok(new { Success = true, Message = Messages.User.RoleAssigned });
    }

    /// <summary>Supprime un rôle d'un utilisateur.</summary>
    [HttpDelete("users/{userId:guid}/roles/{roleName}")]
    public async Task<IActionResult> RemoveUserRole(Guid userId, string roleName)
    {
        var removed = await adminService.RemoveUserRoleAsync(userId, roleName);
        if (!removed) return BadRequest(new { Success = false, Message = Messages.User.RoleFailed });
        return Ok(new { Success = true, Message = Messages.User.RoleRemoved });
    }

    /// <summary>Promouvoir un utilisateur au rôle d'administrateur.</summary>
    [HttpPost("users/{userId:guid}/promote-to-admin")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public async Task<IActionResult> PromoteToAdmin(Guid userId)
    {
        var assigned = await adminService.AssignRoleToUserAsync(userId, RoleConstants.Admin);
        if (!assigned) return BadRequest(new { Success = false, Message = Messages.User.PromoteFailed });
        return Ok(new { Success = true, Message = Messages.User.Promoted });
    }

    /// <summary>Approuve un certificat par son identifiant.</summary>
    [HttpPatch("certificates/{id:guid}/approve")]
    public async Task<IActionResult> ApproveCertificate(Guid id)
    {
        var cert = await certificateService.ApproveCertificateAsync(id);
        if (cert is null) return NotFound(new { Success = false, Message = Messages.Certificate.NotFound });
        return Ok(new { Success = true, Data = cert });
    }

    /// <summary>Rejette un certificat avec une raison donnée.</summary>
    [HttpPatch("certificates/{id:guid}/reject")]
    public async Task<IActionResult> RejectCertificate(Guid id, [FromQuery] string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return BadRequest(new { Success = false, Message = Messages.Certificate.RejectReasonRequired });
        var cert = await certificateService.RejectCertificateAsync(id, reason);
        if (cert is null) return NotFound(new { Success = false, Message = Messages.Certificate.NotFound });
        return Ok(new { Success = true, Data = cert });
    }

    /// <summary>Retourne la liste des certificats avec filtrage par statut.</summary>
    [HttpGet("certificates")]
    public async Task<IActionResult> GetCertificates([FromQuery] string? status)
    {
        return Ok(new { Success = true, Data = await certificateService.GetCertificatesAsync(status) });
    }

    /// <summary>Retourne la liste des articles avec filtrage par statut et pagination.</summary>
    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(new { Success = true, Data = await postQuery.GetAllAsync(status, null, null, null, page, pageSize) });
    }

    /// <summary>Retourne la liste de tous les commentaires.</summary>
    [HttpGet("comments")]
    public async Task<IActionResult> GetComments()
    {
        return Ok(new { Success = true, Data = await commentService.GetAllAsync() });
    }

    /// <summary>Remplace tous les rôles d'un utilisateur par un nouveau rôle.</summary>
    [HttpPut("users/{userId:guid}/roles")]
    public async Task<IActionResult> ReplaceUserRoles(Guid userId, [FromBody] AssignRoleRequest request)
    {
        var replaced = await adminService.ReplaceUserRolesAsync(userId, request.RoleName);
        if (!replaced) return BadRequest(new { Success = false, Message = Messages.User.RoleFailed });
        return Ok(new { Success = true, Message = Messages.User.RoleAssigned });
    }
}
