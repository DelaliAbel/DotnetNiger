using DotnetNiger.Api.DTOs.Responses;
using ErrorResponse  = DotnetNiger.Api.DTOs.Responses.ErrorResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AssignRoleRequest  = DotnetNiger.Api.DTOs.Requests.AssignRoleRequest;
using IdentityDTOs  = DotnetNiger.Api.DTOs.Requests;
using ErrorMessages  = DotnetNiger.Api.Constants.ErrorMessages;
using SuccessMessages  = DotnetNiger.Api.Constants.SuccessMessages;

namespace DotnetNiger.Api.Controllers.Admin;

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

    [HttpPost("invite")]
    [Authorize(Policy = "admin.users.invite")]
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
    [Authorize]
    public async Task<ActionResult<object>> GetMyStats()
    {
        var userId = GetUserId();
        var stats = await _dashboardService.GetMyStatsAsync(userId);
        return Ok(stats);
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
    [Authorize(Policy = "admin.roles.manage")]
    public async Task<ActionResult> AssignRoleToUser(Guid id, [FromBody] AssignRoleRequest request)
    {
        var assigned = await _adminService.AssignRoleToUserAsync(id, request.RoleName);
        if (!assigned)
            return BadRequest(new ErrorResponse(ErrorMessages.UnableToAssignRole));
        return Ok(new { message = SuccessMessages.RoleAssigned });
    }

    [HttpDelete("users/{id:guid}/roles/{roleName}")]
    [Authorize(Policy = "admin.roles.manage")]
    public async Task<ActionResult> RemoveUserRole(Guid id, string roleName)
    {
        var removed = await _adminService.RemoveUserRoleAsync(id, roleName);
        if (!removed)
            return BadRequest(new ErrorResponse(ErrorMessages.UnableToAssignRole));
        return Ok(new { message = SuccessMessages.RoleRemoved });
    }

    [HttpDelete("users/{id:guid}")]
    [Authorize(Policy = "admin.users.delete")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var deleted = await _adminService.DeleteUserAsync(id);
        if (!deleted)
            return NotFound(new ErrorResponse(ErrorMessages.UserNotFound));
        return Ok(new { message = SuccessMessages.UserDeleted });
    }

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
