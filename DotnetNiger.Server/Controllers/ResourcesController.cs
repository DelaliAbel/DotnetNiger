using Asp.Versioning;
using DotnetNiger.Infrastructure.Services;
using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ResourcesController(IResourceQueryService resourceQuery, IResourceCommandService resourceCommand) : BaseController
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
        var result = await resourceQuery.GetAllAsync(resourceType, level, query, tag, categoryId, page, pageSize, after);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var userId = GetUserId();
        var result = await resourceQuery.GetAllAsync(null, null, null, null, null, page, pageSize, null, userId);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var resource = await resourceQuery.GetByIdAsync(id);
        if (resource is null) return NotFound(new { Success = false, Message = Messages.Resource.NotFound });
        return Ok(new { Success = true, Data = resource });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var resource = await resourceQuery.GetBySlugAsync(slug);
        if (resource is null) return NotFound(new { Success = false, Message = Messages.Resource.NotFound });
        return Ok(new { Success = true, Data = resource });
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<OGMetadata>> GetOGBySlug(string slug)
    {
        var resource = await resourceQuery.GetBySlugAsync(slug);
        if (resource is null) return NotFound(new { Success = false, Message = Messages.Resource.NotFound });

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
        var types = await resourceQuery.GetResourceTypesAsync();
        return Ok(new { Success = true, Data = types });
    }

    [HttpGet("levels")]
    public async Task<IActionResult> GetLevels()
    {
        var levels = await resourceQuery.GetLevelsAsync();
        return Ok(new { Success = true, Data = levels });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateResourceRequest request)
    {
        var userId = GetUserId();
        try
        {
            var resource = await resourceCommand.CreateAsync(request, userId, IsAdmin(), IsCollaborator());
            return CreatedAtAction(nameof(GetById), new { id = resource.Id }, new { Success = true, Data = resource });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResourceRequest request)
    {
        try
        {
            var userId = GetUserId();
            var resource = await resourceCommand.UpdateAsync(id, request, userId, IsAdmin());
            if (resource is null) return NotFound(new { Success = false, Message = Messages.Resource.NotFound });
            return Ok(new { Success = true, Data = resource });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Success = false, Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = await resourceCommand.DeleteAsync(id, userId, IsAdmin());
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Resource.NotFound });
        return Ok(new { Success = true, Message = Messages.Resource.Deleted });
    }

    [HttpPost("{id:guid}/views")]
    public async Task<IActionResult> IncrementViewCount(Guid id)
    {
        var resource = await resourceCommand.IncrementViewCountAsync(id);
        if (resource is null) return NotFound(new { Success = false, Message = Messages.Resource.NotFound });
        return Ok(new { Success = true, Data = resource });
    }
}
