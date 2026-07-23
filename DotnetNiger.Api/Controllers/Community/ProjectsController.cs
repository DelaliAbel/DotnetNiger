using DotnetNiger.Api.Services;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Community;

/// <summary>Contrôleur de gestion des projets communautaires.</summary>
[Route("api/projects")]
public class ProjectsController(IProjectService projectService) : BaseController
{
    /// <summary>Récupère la liste paginée des projets avec filtres optionnels.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await projectService.GetAllAsync(status, query, page, pageSize);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>Récupère les projets mis en avant.</summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var projects = await projectService.GetFeaturedAsync();
        return Ok(new { Success = true, Data = projects });
    }

    /// <summary>Récupère un projet par son identifiant.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var project = await projectService.GetByIdAsync(id);
        if (project is null) return NotFound(new { Success = false, Message = Messages.Project.NotFound });
        return Ok(new { Success = true, Data = project });
    }

    /// <summary>Récupère un projet par son slug.</summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var project = await projectService.GetBySlugAsync(slug);
        if (project is null) return NotFound(new { Success = false, Message = Messages.Project.NotFound });
        return Ok(new { Success = true, Data = project });
    }

    /// <summary>Crée un nouveau projet.</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var userId = GetUserId();
        var authorName = GetUserName();
        var project = await projectService.CreateAsync(request, userId, authorName);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, new { Success = true, Data = project });
    }

    /// <summary>Met à jour un projet existant.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var userId = GetUserId();
        var project = await projectService.UpdateAsync(id, request, userId, IsAdmin());
        if (project is null) return NotFound(new { Success = false, Message = Messages.Project.NotFound });
        return Ok(new { Success = true, Data = project });
    }

    /// <summary>Supprime un projet.</summary>
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
