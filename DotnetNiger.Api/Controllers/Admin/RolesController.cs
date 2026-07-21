using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ErrorResponse  = DotnetNiger.Api.DTOs.Responses.ErrorResponse;

namespace DotnetNiger.Api.Controllers.Admin;

[ApiController]

[Route("api/roles")]
[Authorize(Policy = "admin.roles.manage")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly IUserService _userService;

    public RolesController(IRoleService roleService, IUserService userService)
    {
        _roleService = roleService;
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create([FromBody] CreateRoleRequest request)
    {
        var role = await _roleService.CreateAsync(request);
        return CreatedAtAction(nameof(GetAll), role);
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<RoleResponse>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var roles = await _roleService.GetAllAsync(new PaginationQuery(page, pageSize));
        return Ok(roles);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> Update(Guid id,
        [FromBody] UpdateRoleRequest request)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role == null) return NotFound();
        var updated = await _roleService.UpdateAsync(id, request);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _roleService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{roleId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> AssignUser(Guid roleId, Guid userId)
    {
        await _roleService.AssignToUserAsync(userId, roleId);
        return NoContent();
    }

    [HttpDelete("{roleId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> RemoveUser(Guid roleId, Guid userId)
    {
        await _roleService.RemoveFromUserAsync(userId, roleId);
        return NoContent();
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<RoleResponse>>> GetUserRoles(Guid userId)
    {
        var roles = await _roleService.GetUserRolesAsync(userId);
        return Ok(roles);
    }
}
