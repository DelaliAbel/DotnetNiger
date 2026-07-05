using DotnetNiger.Common.DTOs.Responses;
using ErrorResponse = DotnetNiger.Identity.Application.DTOs.Responses.ErrorResponse;
using DotnetNiger.Identity.Application.Services;
using DotnetNiger.Identity.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AssignRoleRequest = DotnetNiger.Common.DTOs.Requests.AssignRoleRequest;
using IdentityDTOs = DotnetNiger.Identity.Application.DTOs.Requests;
using RoleConstants = DotnetNiger.Common.Constants.RoleConstants;
using ErrorMessages = DotnetNiger.Common.Constants.ErrorMessages;
using SuccessMessages = DotnetNiger.Common.Constants.SuccessMessages;

namespace DotnetNiger.Identity.Api.Controllers;

/// <summary>Administration système : statistiques, utilisateurs et métriques globales (Admin).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly DashboardService _dashboardService;

    public AdminController(IAdminService adminService, DashboardService dashboardService)
    {
        _adminService = adminService;
        _dashboardService = dashboardService;
    }

    /// <summary>Invite un administrateur par email.</summary>
    [HttpPost("invite")]
    public async Task<ActionResult> Invite([FromBody] IdentityDTOs.InviteAdminRequest request)
    {
        await _adminService.InviteAsync(request.Email, request.Role);
        return Ok(new { message = SuccessMessages.InvitationSent });
    }

    /// <summary>Statistiques système (tenants, utilisateurs, rôles, permissions).</summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var stats = await _dashboardService.GetSystemStatsAsync();
        return Ok(stats);
    }

    /// <summary>Historique de connexion d'un tenant.</summary>
    [HttpGet("tenants/{tenantId:guid}/login-history")]
    public async Task<ActionResult> GetTenantLoginHistory(Guid tenantId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _dashboardService.GetTenantLoginHistoryAsync(tenantId,
            Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
        return Ok(result);
    }

    /// <summary>Journaux d'audit avec filtres.</summary>
    [HttpGet("audit-logs")]
    public async Task<ActionResult<PaginatedResponse<AuditLog>>> GetAuditLogs(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? entityType = null, [FromQuery] string? action = null,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var result = await _dashboardService.GetAuditLogsAsync(
            Math.Max(1, page), Math.Clamp(pageSize, 1, 100),
            entityType, action, from, to);
        return Ok(result);
    }

    /// <summary>Liste de tous les utilisateurs (tous tenants).</summary>
    [HttpGet("users")]
    public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAcrossTenantsAsync();
        return Ok(users);
    }

    /// <summary>Détail d'un utilisateur.</summary>
    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetUserById(Guid id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(user);
    }

    /// <summary>Active ou désactive un utilisateur.</summary>
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

    /// <summary>Assigne un rôle à un utilisateur.</summary>
    [HttpPost("users/{id:guid}/roles")]
    public async Task<ActionResult> AssignRoleToUser(Guid id, [FromBody] AssignRoleRequest request)
    {
        var assigned = await _adminService.AssignRoleToUserAsync(id, request.RoleName);
        if (!assigned)
            return BadRequest(new ErrorResponse(ErrorMessages.UnableToAssignRole));
        return Ok(new { message = SuccessMessages.RoleAssigned });
    }

    /// <summary>Supprime un utilisateur.</summary>
    [HttpDelete("users/{id:guid}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var deleted = await _adminService.DeleteUserAsync(id);
        if (!deleted)
            return NotFound(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(new { message = SuccessMessages.UserDeleted });
    }

    /// <summary>Crée un utilisateur (admin).</summary>
    [HttpPost("users")]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] IdentityDTOs.AdminCreateUserRequest request)
    {
        var user = await _adminService.CreateUserAsync(request);
        if (user == null)
            return BadRequest(new ErrorResponse(ErrorMessages.TenantNotFound));
        return Ok(user);
    }
}
