using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AssignRoleRequest  = DotnetNiger.Api.Application.DTOs.Requests.AssignRoleRequest;
using IdentityDTOs  = DotnetNiger.Api.Application.DTOs.Requests;
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
    public async Task<IActionResult> Invite([FromBody] IdentityDTOs.InviteAdminRequest request, CancellationToken ct = default)
    {
        await _adminService.InviteAsync(request.Email, request.Role, ct);
        return Success<object?>(null, SuccessMessages.InvitationSent);
    }

    /// <summary>Récupère les statistiques globales du système.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var stats = await _dashboardService.GetSystemStatsAsync(ct);
        return Success(stats);
    }

    /// <summary>Récupère les statistiques personnelles de l'utilisateur connecté.</summary>
    [HttpGet("stats/mine")]
    public async Task<IActionResult> GetMyStats(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var stats = await _dashboardService.GetMyStatsAsync(userId, ct);
        return Success(stats);
    }

    /// <summary>Récupère la liste de tous les utilisateurs.</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct = default)
    {
        var users = await _adminService.GetAllUsersAsync(ct);
        return Success(users);
    }

    /// <summary>Récupère un utilisateur par son identifiant.</summary>
    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct = default)
    {
        var user = await _adminService.GetUserByIdAsync(id, ct);
        if (user == null)
            return NotFound(ErrorMessages.UserNotFound);
        return Success(user);
    }

    /// <summary>Met à jour le statut (actif/inactif) d'un utilisateur.</summary>
    [HttpPatch("users/{id:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] IdentityDTOs.UpdateUserRequest request, CancellationToken ct = default)
    {
        var updated = await _adminService.UpdateUserStatusAsync(id, request.IsActive ?? true, ct);
        if (!updated)
            return NotFound(ErrorMessages.UserNotFound);
        return Success<object?>(null, SuccessMessages.StatusUpdated);
    }

    /// <summary>Met à jour le profil d'un utilisateur.</summary>
    [HttpPatch("users/{id:guid}/profile")]
    public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] IdentityDTOs.UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _adminService.UpdateUserProfileAsync(id, request, ct);
        if (user == null)
            return NotFound(ErrorMessages.UserNotFound);
        return Success(user);
    }

    /// <summary>Attribue un rôle à un utilisateur.</summary>
    [HttpPost("users/{id:guid}/roles")]
    [Authorize(Policy = "admin.roles.manage")]
    public async Task<IActionResult> AssignRoleToUser(Guid id, [FromBody] AssignRoleRequest request, CancellationToken ct = default)
    {
        var assigned = await _adminService.AssignRoleToUserAsync(id, request.RoleName, ct);
        if (!assigned)
            return BadRequest(ErrorMessages.UnableToAssignRole);
        return Success<object?>(null, SuccessMessages.RoleAssigned);
    }

    /// <summary>Supprime un rôle d'un utilisateur.</summary>
    [HttpDelete("users/{id:guid}/roles/{roleName}")]
    [Authorize(Policy = "admin.roles.manage")]
    public async Task<IActionResult> RemoveUserRole(Guid id, string roleName, CancellationToken ct = default)
    {
        var removed = await _adminService.RemoveUserRoleAsync(id, roleName, ct);
        if (!removed)
            return BadRequest(ErrorMessages.UnableToAssignRole);
        return Success<object?>(null, SuccessMessages.RoleRemoved);
    }

    /// <summary>Supprime un utilisateur par son identifiant.</summary>
    [HttpDelete("users/{id:guid}")]
    [Authorize(Policy = "admin.users.delete")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct = default)
    {
        try
        {
            var callerId = GetUserId();
            var deleted = await _adminService.DeleteUserAsync(id, callerId, ct);
            if (!deleted)
                return NotFound(ErrorMessages.UserNotFound);
            return Success<object?>(null, SuccessMessages.UserDeleted);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Crée un nouvel utilisateur depuis le panneau admin.</summary>
    [HttpPost("users")]
    [Authorize(Policy = "admin.users.create")]
    public async Task<IActionResult> CreateUser([FromBody] IdentityDTOs.AdminCreateUserRequest request, CancellationToken ct = default)
    {
        var user = await _adminService.CreateUserAsync(request, ct);
        if (user == null)
            return BadRequest(ErrorMessages.UserNotFound);
        return Success(user);
    }

    /// <summary>Met à jour le statut d'appartenance à l'équipe d'un utilisateur.</summary>
    [HttpPatch("users/{id:guid}/team")]
    [Authorize(Policy = "admin.users.create")]
    public async Task<IActionResult> UpdateUserTeam(Guid id, [FromBody] IdentityDTOs.UpdateTeamRequest request, CancellationToken ct = default)
    {
        var updated = await _adminService.UpdateUserTeamAsync(id, request.IsTeamMember, request.Position ?? string.Empty, ct);
        if (!updated)
            return NotFound(ErrorMessages.UserNotFound);
        return Success<object?>(null, "Statut d'équipe mis à jour");
    }
}
