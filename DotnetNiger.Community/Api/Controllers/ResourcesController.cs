using Asp.Versioning;
using System.Security.Claims;
using DotnetNiger.Community.Application;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ResourcesController(IResourceService resourceService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? resourceType, [FromQuery] string? level, [FromQuery] string? query,
        [FromQuery] string? tag, [FromQuery] Guid? categoryId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] Guid? after = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await resourceService.GetAllAsync(resourceType, level, query, tag, categoryId, page, pageSize, after);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var resource = await resourceService.GetByIdAsync(id);
        if (resource is null) return NotFound(new { Success = false, Message = "Resource not found" });
        return Ok(new { Success = true, Data = resource });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var resource = await resourceService.GetBySlugAsync(slug);
        if (resource is null) return NotFound(new { Success = false, Message = "Resource not found" });
        return Ok(new { Success = true, Data = resource });
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<OGMetadata>> GetOGBySlug(string slug)
    {
        var resource = await resourceService.GetBySlugAsync(slug);
        if (resource is null) return NotFound(new { Success = false, Message = "Resource not found" });

        return Ok(new ApiSuccessResponse<OGMetadata>
        {
            Data = new OGMetadata
            {
                Title = resource.Title,
                Description = resource.Description,
                ImageUrl = string.Empty,
                UpdatedAt = resource.UpdatedAt
            }
        });
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetTypes()
    {
        var types = await resourceService.GetResourceTypesAsync();
        return Ok(new { Success = true, Data = types });
    }

    [HttpGet("levels")]
    public async Task<IActionResult> GetLevels()
    {
        var levels = await resourceService.GetLevelsAsync();
        return Ok(new { Success = true, Data = levels });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateResourceRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(new { Success = false, Message = "Invalid user identity" });

        var resource = await resourceService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, new { Success = true, Data = resource });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateResourceRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(new { Success = false, Message = "Invalid user identity" });

        var isAdmin = User.IsInRole(RoleConstants.Admin);
        var resource = await resourceService.UpdateAsync(id, request, userId, isAdmin);
        if (resource is null) return NotFound(new { Success = false, Message = "Resource not found" });
        return Ok(new { Success = true, Data = resource });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(new { Success = false, Message = "Invalid user identity" });

        var isAdmin = User.IsInRole(RoleConstants.Admin);
        var deleted = await resourceService.DeleteAsync(id, userId, isAdmin);
        if (!deleted) return NotFound(new { Success = false, Message = "Resource not found" });
        return Ok(new { Success = true, Message = "Resource deleted" });
    }

    [HttpPost("{id:guid}/views")]
    public async Task<IActionResult> IncrementViewCount(Guid id)
    {
        var resource = await resourceService.IncrementViewCountAsync(id);
        if (resource is null) return NotFound(new { Success = false, Message = "Resource not found" });
        return Ok(new { Success = true, Data = resource });
    }
}
