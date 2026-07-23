using DotnetNiger.Api.DTOs.Responses;
using ErrorResponse  = DotnetNiger.Api.DTOs.Responses.ErrorResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AssignRoleRequest  = DotnetNiger.Api.DTOs.Requests.AssignRoleRequest;
using IdentityDTOs  = DotnetNiger.Api.DTOs.Requests;
using ErrorMessages  = DotnetNiger.Api.Constants.ErrorMessages;
using SuccessMessages  = DotnetNiger.Api.Constants.SuccessMessages;

namespace DotnetNiger.Api.Controllers.Admin;

/// <summary>Contrôleur d'administration pour la gestion des utilisateurs et du tableau de bord.</summary>
[ApiController]
[Route("api/admin")]
[Authorize(Policy = "admin.dashboard.view")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;
    private readonly DashboardService _dashboardService;

    public AdminController(IAdminService adminService, DashboardService dashboardService)
    {
        _adminService = adminService;
        _dashboardService = dashboardService;
    }

    /// <summary>Envoie une invitation à un nouvel administrateur.</summary>
    [HttpPost("invite")]
    [Authorize(Policy = "admin.users.invite")]
    public async Task<ActionResult> Invite([FromBody] IdentityDTOs.InviteAdminRequest request)
    {
        await _adminService.InviteAsync(request.Email, request.Role);
        return Ok(new { message = SuccessMessages.InvitationSent });
    }

    /// <summary>Récupère les statistiques globales du système.</summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var stats = await _dashboardService.GetSystemStatsAsync();
        return Ok(stats);
    }

    /// <summary>Récupère les statistiques personnelles de l'utilisateur connecté.</summary>
    [HttpGet("stats/mine")]
    [Authorize]
    public async Task<ActionResult<object>> GetMyStats()
    {
        var userId = GetUserId();
        var stats = await _dashboardService.GetMyStatsAsync(userId);
        return Ok(stats);
    }

    /// <summary>Récupère la liste de tous les utilisateurs.</summary>
    [HttpGet("users")]
    public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>Récupère un utilisateur par son identifiant.</summary>
    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetUserById(Guid id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(user);
    }

    /// <summary>Met à jour le statut (actif/inactif) d'un utilisateur.</summary>
    [HttpPatch("users/{id:guid}/status")]
    public async Task<ActionResult> UpdateUserStatus(Guid id, [FromBody] IdentityDTOs.UpdateUserRequest request)
    {
        var updated = await _adminService.UpdateUserStatusAsync(id, request.IsActive ?? true);
        if (!updated)
            return NotFound(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(new { message = SuccessMessages.StatusUpdated });
    }

    /// <summary>Met à jour le profil d'un utilisateur.</summary>
    [HttpPatch("users/{id:guid}/profile")]
    public async Task<ActionResult<UserResponse>> UpdateUserProfile(Guid id, [FromBody] IdentityDTOs.UpdateUserRequest request)
    {
        var user = await _adminService.UpdateUserProfileAsync(id, request);
        if (user == null)
            return NotFound(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(user);
    }

    /// <summary>Attribue un rôle à un utilisateur.</summary>
    [HttpPost("users/{id:guid}/roles")]
    [Authorize(Policy = "admin.roles.manage")]
    public async Task<ActionResult> AssignRoleToUser(Guid id, [FromBody] AssignRoleRequest request)
    {
        var assigned = await _adminService.AssignRoleToUserAsync(id, request.RoleName);
        if (!assigned)
            return BadRequest(new ErrorResponse(ErrorMessages.UnableToAssignRole));
        return Ok(new { message = SuccessMessages.RoleAssigned });
    }

    /// <summary>Supprime un rôle d'un utilisateur.</summary>
    [HttpDelete("users/{id:guid}/roles/{roleName}")]
    [Authorize(Policy = "admin.roles.manage")]
    public async Task<ActionResult> RemoveUserRole(Guid id, string roleName)
    {
        var removed = await _adminService.RemoveUserRoleAsync(id, roleName);
        if (!removed)
            return BadRequest(new ErrorResponse(ErrorMessages.UnableToAssignRole));
        return Ok(new { message = SuccessMessages.RoleRemoved });
    }

    /// <summary>Supprime un utilisateur par son identifiant.</summary>
    [HttpDelete("users/{id:guid}")]
    [Authorize(Policy = "admin.users.delete")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        try
        {
            var callerId = GetUserId();
            var deleted = await _adminService.DeleteUserAsync(id, callerId);
            if (!deleted)
                return NotFound(new ErrorResponse(ErrorMessages.UserNotFound));
            return Ok(new { message = SuccessMessages.UserDeleted });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>Crée un nouvel utilisateur depuis le panneau admin.</summary>
    [HttpPost("users")]
    [Authorize(Policy = "admin.users.create")]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] IdentityDTOs.AdminCreateUserRequest request)
    {
        var user = await _adminService.CreateUserAsync(request);
        if (user == null)
            return BadRequest(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(user);
    }
}
