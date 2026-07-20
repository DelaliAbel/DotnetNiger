using DotnetNiger.Domain.DTOs.Responses;
using ErrorResponse = DotnetNiger.Domain.DTOs.Responses.ErrorResponse;
using DotnetNiger.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AssignRoleRequest = DotnetNiger.Domain.DTOs.Requests.AssignRoleRequest;
using IdentityDTOs = DotnetNiger.Domain.DTOs.Requests;
using RoleConstants = DotnetNiger.Domain.Constants.RoleConstants;
using ErrorMessages = DotnetNiger.Domain.Constants.ErrorMessages;
using SuccessMessages = DotnetNiger.Domain.Constants.SuccessMessages;

namespace DotnetNiger.Server.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;
    private readonly DashboardService _dashboardService;

    public AdminController(IAdminService adminService, DashboardService dashboardService)
    {
        _adminService = adminService;
        _dashboardService = dashboardService;
    }

    [HttpPost("invite")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public async Task<ActionResult> Invite([FromBody] IdentityDTOs.InviteAdminRequest request)
    {
        await _adminService.InviteAsync(request.Email, request.Role);
        return Ok(new { message = SuccessMessages.InvitationSent });
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var stats = await _dashboardService.GetSystemStatsAsync();
        return Ok(stats);
    }

    [HttpGet("stats/mine")]
    [Authorize(Roles = "Collaborator,Admin,SuperAdmin")]
    public async Task<ActionResult<object>> GetMyStats()
    {
        var userId = GetUserId();
        var stats = await _dashboardService.GetMyStatsAsync(userId);
        return Ok(stats);
    }

    [HttpGet("login-history")]
    public async Task<ActionResult> GetLoginHistory(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _dashboardService.GetLoginHistoryAsync(
            Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
        return Ok(result);
    }

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

    [HttpGet("users")]
    public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetUserById(Guid id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(user);
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<ActionResult> UpdateUserStatus(Guid id, [FromBody] IdentityDTOs.UpdateUserRequest request)
    {
        var updated = await _adminService.UpdateUserStatusAsync(id, request.IsActive ?? true);
        if (!updated)
            return NotFound(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(new { message = SuccessMessages.StatusUpdated });
    }

    [HttpPatch("users/{id:guid}/profile")]
    public async Task<ActionResult<UserResponse>> UpdateUserProfile(Guid id, [FromBody] IdentityDTOs.UpdateUserRequest request)
    {
        var user = await _adminService.UpdateUserProfileAsync(id, request);
        if (user == null)
            return NotFound(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(user);
    }

    [HttpPost("users/{id:guid}/roles")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public async Task<ActionResult> AssignRoleToUser(Guid id, [FromBody] AssignRoleRequest request)
    {
        var assigned = await _adminService.AssignRoleToUserAsync(id, request.RoleName);
        if (!assigned)
            return BadRequest(new ErrorResponse(ErrorMessages.UnableToAssignRole));
        return Ok(new { message = SuccessMessages.RoleAssigned });
    }

    [HttpDelete("users/{id:guid}/roles/{roleName}")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public async Task<ActionResult> RemoveUserRole(Guid id, string roleName)
    {
        var removed = await _adminService.RemoveUserRoleAsync(id, roleName);
        if (!removed)
            return BadRequest(new ErrorResponse(ErrorMessages.UnableToAssignRole));
        return Ok(new { message = SuccessMessages.RoleRemoved });
    }

    [HttpDelete("users/{id:guid}")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var deleted = await _adminService.DeleteUserAsync(id);
        if (!deleted)
            return NotFound(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(new { message = SuccessMessages.UserDeleted });
    }

    [HttpPost("users")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] IdentityDTOs.AdminCreateUserRequest request)
    {
        var user = await _adminService.CreateUserAsync(request);
        if (user == null)
            return BadRequest(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(user);
    }
}
