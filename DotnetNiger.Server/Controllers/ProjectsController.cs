using Asp.Versioning;
using DotnetNiger.Infrastructure.Services;
using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProjectsController(IProjectService projectService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await projectService.GetAllAsync(status, query, page, pageSize);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var projects = await projectService.GetFeaturedAsync();
        return Ok(new { Success = true, Data = projects });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var project = await projectService.GetByIdAsync(id);
        if (project is null) return NotFound(new { Success = false, Message = Messages.Project.NotFound });
        return Ok(new { Success = true, Data = project });
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var project = await projectService.GetBySlugAsync(slug);
        if (project is null) return NotFound(new { Success = false, Message = Messages.Project.NotFound });
        return Ok(new { Success = true, Data = project });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var userId = GetUserId();
        var authorName = GetUserName();
        var project = await projectService.CreateAsync(request, userId, authorName);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, new { Success = true, Data = project });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var userId = GetUserId();
        var project = await projectService.UpdateAsync(id, request, userId, IsAdmin());
        if (project is null) return NotFound(new { Success = false, Message = Messages.Project.NotFound });
        return Ok(new { Success = true, Data = project });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = await projectService.DeleteAsync(id, userId, IsAdmin());
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Project.NotFound });
        return Ok(new { Success = true, Message = Messages.Project.Deleted });
    }
}
