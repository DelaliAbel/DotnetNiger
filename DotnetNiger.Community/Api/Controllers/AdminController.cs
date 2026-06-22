using Asp.Versioning;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using DotnetNiger.Community.Application;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = RoleConstants.Admin)]
public class AdminController(IAdminService adminService, IEventService eventService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var stats = await adminService.GetDashboardAsync();
        return Ok(new { Success = true, Data = stats });
    }

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

    [HttpPatch("events/{id:guid}/publish")]
    public async Task<IActionResult> PublishEvent(Guid id)
    {
        var ev = await eventService.PublishAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = "Event not found" });
        return Ok(new { Success = true, Data = ev });
    }

    [HttpPatch("events/{id:guid}/unpublish")]
    public async Task<IActionResult> UnpublishEvent(Guid id)
    {
        var ev = await eventService.UnpublishAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = "Event not found" });
        return Ok(new { Success = true, Data = ev });
    }

    [HttpPatch("events/{id:guid}/approve")]
    public async Task<IActionResult> ApproveEvent(Guid id)
    {
        var ev = await eventService.ApproveAsync(id);
        if (ev is null) return NotFound(new { Success = false, Message = "Event not found" });
        return Ok(new { Success = true, Data = ev });
    }

    [HttpPatch("events/{id:guid}/reject")]
    public async Task<IActionResult> RejectEvent(Guid id, [FromQuery] string reason)
    {
        var ev = await eventService.RejectAsync(id, reason);
        if (ev is null) return NotFound(new { Success = false, Message = "Event not found" });
        return Ok(new { Success = true, Message = "Event rejected", Data = ev });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await adminService.GetUsersAsync();
        return Ok(new { Success = true, Data = users });
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await adminService.GetUserAsync(id);
        if (user is null) return NotFound(new { Success = false, Message = "User not found" });
        return Ok(new { Success = true, Data = user });
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        var updated = await adminService.UpdateUserStatusAsync(id, request.IsActive);
        if (!updated) return NotFound(new { Success = false, Message = "User not found" });
        return Ok(new { Success = true, Message = "User status updated" });
    }

    [HttpPatch("users/{id:guid}/team")]
    public async Task<IActionResult> UpdateUserTeam(Guid id, [FromBody] UpdateUserTeamRequest request)
    {
        var updated = await adminService.UpdateUserTeamAsync(id, request.IsTeamMember, request.Position);
        if (!updated) return NotFound(new { Success = false, Message = "User not found" });
        return Ok(new { Success = true, Message = "Team status updated" });
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserRequest request)
    {
        var user = await adminService.CreateUserAsync(request);
        if (user is null) return BadRequest(new { Success = false, Message = "Failed to create user" });
        return Ok(new { Success = true, Data = user });
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var deleted = await adminService.DeleteUserAsync(id);
        if (!deleted) return NotFound(new { Success = false, Message = "User not found" });
        return Ok(new { Success = true, Message = "User deleted" });
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await adminService.GetRolesAsync();
        return Ok(new { Success = true, Data = roles });
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var role = await adminService.CreateRoleAsync(request.Name);
        if (role is null) return BadRequest(new { Success = false, Message = "Failed to create role" });
        return Ok(new { Success = true, Data = role });
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await adminService.GetPermissionsAsync();
        return Ok(new { Success = true, Data = permissions });
    }

    [HttpPost("permissions")]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
    {
        var permission = await adminService.CreatePermissionAsync(request.Name, request.Description);
        if (permission is null) return BadRequest(new { Success = false, Message = "Failed to create permission" });
        return Ok(new { Success = true, Data = permission });
    }

    [HttpPost("roles/{roleId:guid}/permissions")]
    public async Task<IActionResult> AssignPermissionToRole(Guid roleId, [FromBody] AssignPermissionRequest request)
    {
        var assigned = await adminService.AssignPermissionToRoleAsync(roleId, request.PermissionId);
        if (!assigned) return BadRequest(new { Success = false, Message = "Failed to assign permission" });
        return Ok(new { Success = true, Message = "Permission assigned" });
    }

    [HttpPost("users/{userId:guid}/roles")]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, [FromBody] AssignRoleRequest request)
    {
        var assigned = await adminService.AssignRoleToUserAsync(userId, request.RoleName);
        if (!assigned) return BadRequest(new { Success = false, Message = "Failed to assign role" });
        return Ok(new { Success = true, Message = "Role assigned" });
    }
}
