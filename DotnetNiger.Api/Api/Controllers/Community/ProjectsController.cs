using System.Threading;
using DotnetNiger.Api.Application.Interfaces;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Community;

/// <summary>Contrôleur de gestion des projets communautaires.</summary>
[ApiController]
[Route("api/projects")]
public class ProjectsController(IProjectService projectService) : BaseController
{
    /// <summary>Récupère la liste paginée des projets avec filtres optionnels.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await projectService.GetAllAsync(status, query, page, pageSize, ct: ct);
        return Success(result);
    }

    /// <summary>Récupère les projets de l'utilisateur connecté.</summary>
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var userId = GetUserId();
        var result = await projectService.GetAllAsync(null, null, page, pageSize, userId, ct);
        return Success(result);
    }

    /// <summary>Récupère les projets mis en avant.</summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured(CancellationToken ct = default)
    {
        var projects = await projectService.GetFeaturedAsync(ct);
        return Success(projects);
    }

    /// <summary>Récupère un projet par son identifiant.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var project = await projectService.GetByIdAsync(id, ct);
        if (project is null) return NotFound(Messages.Project.NotFound);
        return Success(project);
    }

    /// <summary>Récupère un projet par son slug.</summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct = default)
    {
        var project = await projectService.GetBySlugAsync(slug, ct);
        if (project is null) return NotFound(Messages.Project.NotFound);
        return Success(project);
    }

    /// <summary>Crée un nouveau projet.</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var authorName = GetUserName();
        var project = await projectService.CreateAsync(request, userId, authorName, ct);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, new { success = true, data = project, message = (string?)null });
    }

    /// <summary>Met à jour un projet existant.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var project = await projectService.UpdateAsync(id, request, userId, IsAdmin(), ct);
        if (project is null) return NotFound(Messages.Project.NotFound);
        return Success(project);
    }

    /// <summary>Supprime un projet.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var deleted = await projectService.DeleteAsync(id, userId, IsAdmin(), ct);
        if (!deleted) return NotFound(Messages.Project.NotFound);
        return Success<object?>(null, Messages.Project.Deleted);
    }
}
