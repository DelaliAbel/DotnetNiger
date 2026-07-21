using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Community;

[ApiController]
[Route("api/tags")]
public class TagsController(ITagService tagService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tags = await tagService.GetAllAsync();
        return Ok(new { Success = true, Data = tags });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var t = await tagService.GetByIdAsync(id);
        if (t is null) return NotFound(new { Success = false, Message = Messages.Tag.NotFound });
        return Ok(new { Success = true, Data = t });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var t = await tagService.GetBySlugAsync(slug);
        if (t is null) return NotFound(new { Success = false, Message = Messages.Tag.NotFound });
        return Ok(new { Success = true, Data = t });
    }

    [HttpPost]
    [Authorize(Policy = "community.tags.manage")]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
    {
        var t = await tagService.CreateAsync(request.Name);
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, new { Success = true, Data = t });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "community.tags.manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateTagRequest request)
    {
        var t = await tagService.UpdateAsync(id, request.Name);
        if (t is null) return NotFound(new { Success = false, Message = Messages.Tag.NotFound });
        return Ok(new { Success = true, Data = t });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "community.tags.manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await tagService.DeleteAsync(id);
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Tag.NotFound });
        return Ok(new { Success = true, Message = Messages.Tag.Deleted });
    }
}
