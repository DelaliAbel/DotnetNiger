using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ErrorResponse = DotnetNiger.Domain.DTOs.Responses.ErrorResponse;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService) => _permissionService = permissionService;

    [HttpPost]
    public async Task<ActionResult<PermissionResponse>> Create(
        [FromBody] CreatePermissionRequest request)
    {
        var permission = await _permissionService.CreateAsync(request);
        return CreatedAtAction(nameof(GetAll), permission);
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PermissionResponse>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var permissions = await _permissionService.GetAllAsync(new PaginationQuery(page, pageSize));
        return Ok(permissions);
    }

    [HttpGet("grouped")]
    public async Task<ActionResult<List<PermissionGroupResponse>>> GetGrouped()
    {
        var grouped = await _permissionService.GetGroupedAsync();
        return Ok(grouped);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var permission = await _permissionService.GetByIdAsync(id);
        if (permission == null) return NotFound();
        await _permissionService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignToRole(
        [FromBody] AssignPermissionsRequest request)
    {
        await _permissionService.AssignToRoleAsync(request.RoleId, request.PermissionIds.ToList());
        return NoContent();
    }
}
