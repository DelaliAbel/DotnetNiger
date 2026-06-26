using Asp.Versioning;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Application.Constants;
using Microsoft.AspNetCore.Authorization;
using DotnetNiger.Community.Application;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Points d'accès pour l'administration de la plateforme : utilisateurs, événements et certificats.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
public class AdminController(IAdminService adminService, IEventService eventService, IProfileService profileService) : ControllerBase
{
    /// <summary>Retourne les statistiques du tableau de bord d'administration.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var stats = await adminService.GetDashboardAsync();
        return Ok(new { Success = true, Data = stats });
    }

    /// <summary>Récupère la liste des événements, avec un filtre optionnel sur le statut.</summary>
    /// <param name="status">Filtre par statut (ex: "pending").</param>
    /// <param name="page">Numéro de la page (défaut: 1).</param>
    /// <param name="pageSize">Nombre d'éléments par page (défaut: 10).</param>
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (status == "pending")
        {
            var events = await eventService.GetPendingEventsAsync(page, pageSize);
            return Ok(new { Success = true, Data = events });
        }

        var paginated = await eventService.GetAllAsync(null, null, null, null, null, null, null, null, page, pageSize);
        return Ok(new { Success = true, Data = paginated });
    }

    /// <summary>Publie un événement sur la plateforme.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    [HttpPatch("events/{id:guid}/publish")]
    public async Task<IActionResult> PublishEvent(Guid id)
    {
        var ev = await eventService.PublishAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    /// <summary>Dépublie un événement.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    [HttpPatch("events/{id:guid}/unpublish")]
    public async Task<IActionResult> UnpublishEvent(Guid id)
    {
        var ev = await eventService.UnpublishAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    /// <summary>Approuve un événement en attente de validation.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    [HttpPatch("events/{id:guid}/approve")]
    public async Task<IActionResult> ApproveEvent(Guid id)
    {
        var ev = await eventService.ApproveAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Data = ev });
    }

    /// <summary>Rejette un événement avec un motif.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="reason">Raison du rejet.</param>
    [HttpPatch("events/{id:guid}/reject")]
    public async Task<IActionResult> RejectEvent(Guid id, [FromQuery] string reason)
    {
        var ev = await eventService.RejectAsync(id, reason);
        if (ev is null) return NotFound(new { Success = false, Message = Messages.Event.NotFound });
        return Ok(new { Success = true, Message = Messages.Event.Rejected, Data = ev });
    }

    /// <summary>Récupère la liste de tous les utilisateurs inscrits.</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await adminService.GetUsersAsync();
        return Ok(new { Success = true, Data = users });
    }

    /// <summary>Recherche un utilisateur par son identifiant.</summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await adminService.GetUserAsync(id);
        if (user is null) return NotFound(new { Success = false, Message = Messages.User.NotFound });
        return Ok(new { Success = true, Data = user });
    }

    /// <summary>Active ou désactive un utilisateur.</summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <param name="request">Nouveau statut d'activation.</param>
    [HttpPatch("users/{id:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        var updated = await adminService.UpdateUserStatusAsync(id, request.IsActive);
        if (!updated) return NotFound(new { Success = false, Message = Messages.User.NotFound });
        return Ok(new { Success = true, Message = Messages.User.StatusUpdated });
    }

    /// <summary>Ajoute ou retire un utilisateur de l'équipe et définit son poste.</summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <param name="request">Informations d'appartenance à l'équipe.</param>
    [HttpPatch("users/{id:guid}/team")]
    public async Task<IActionResult> UpdateUserTeam(Guid id, [FromBody] UpdateUserTeamRequest request)
    {
        var updated = await adminService.UpdateUserTeamAsync(id, request.IsTeamMember, request.Position);
        if (!updated) return NotFound(new { Success = false, Message = Messages.User.NotFound });
        return Ok(new { Success = true, Message = Messages.User.TeamUpdated });
    }

    /// <summary>Crée un nouvel utilisateur (admin, collaborateur).</summary>
    /// <param name="request">Informations du nouvel utilisateur.</param>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserRequest request)
    {
        var user = await adminService.CreateUserAsync(request);
        if (user is null) return BadRequest(new { Success = false, Message = Messages.User.CreateFailed });
        return Ok(new { Success = true, Data = user });
    }

    /// <summary>Supprime définitivement un utilisateur.</summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var deleted = await adminService.DeleteUserAsync(id);
        if (!deleted) return NotFound(new { Success = false, Message = Messages.User.NotFound });
        return Ok(new { Success = true, Message = Messages.User.Deleted });
    }

    /// <summary>Assigne un rôle à un utilisateur.</summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="request">Nom du rôle à assigner.</param>
    [HttpPost("users/{userId:guid}/roles")]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, [FromBody] AssignRoleRequest request)
    {
        var assigned = await adminService.AssignRoleToUserAsync(userId, request.RoleName);
        if (!assigned) return BadRequest(new { Success = false, Message = Messages.User.RoleFailed });
        return Ok(new { Success = true, Message = Messages.User.RoleAssigned });
    }

    /// <summary>Promet un utilisateur au rôle d'administrateur (réservé SuperAdmin).</summary>
    /// <param name="userId">Identifiant de l'utilisateur à promouvoir.</param>
    [HttpPost("users/{userId:guid}/promote-to-admin")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public async Task<IActionResult> PromoteToAdmin(Guid userId)
    {
        var assigned = await adminService.AssignRoleToUserAsync(userId, RoleConstants.Admin);
        if (!assigned) return BadRequest(new { Success = false, Message = Messages.User.PromoteFailed });
        return Ok(new { Success = true, Message = Messages.User.Promoted });
    }

    /// <summary>Approuve un certificat soumis par un membre.</summary>
    /// <param name="id">Identifiant du certificat.</param>
    [HttpPatch("certificates/{id:guid}/approve")]
    public async Task<IActionResult> ApproveCertificate(Guid id)
    {
        var cert = await profileService.ApproveCertificateAsync(id);
        if (cert is null) return NotFound(new { Success = false, Message = Messages.Certificate.NotFound });
        return Ok(new { Success = true, Data = cert });
    }

    /// <summary>Rejette un certificat avec un motif.</summary>
    /// <param name="id">Identifiant du certificat.</param>
    /// <param name="reason">Raison du rejet.</param>
    [HttpPatch("certificates/{id:guid}/reject")]
    public async Task<IActionResult> RejectCertificate(Guid id, [FromQuery] string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return BadRequest(new { Success = false, Message = Messages.Certificate.RejectReasonRequired });

        var cert = await profileService.RejectCertificateAsync(id, reason);
        if (cert is null) return NotFound(new { Success = false, Message = Messages.Certificate.NotFound });
        return Ok(new { Success = true, Data = cert });
    }
}
