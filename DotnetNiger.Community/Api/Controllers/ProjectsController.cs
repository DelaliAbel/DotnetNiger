using Asp.Versioning;
using DotnetNiger.Community.Application;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Common.Constants;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des projets open source de la communauté.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProjectsController(IProjectService projectService) : BaseController
{
    /// <summary>Recherche et filtre les projets avec pagination.</summary>
    /// <param name="status">Filtre par statut (active, beta...).</param>
    /// <param name="query">Recherche textuelle.</param>
    /// <param name="page">Page demandée.</param>
    /// <param name="pageSize">Taille de la page.</param>
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

    /// <summary>Recherche un projet par son identifiant.</summary>
    /// <param name="id">Identifiant du projet.</param>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var project = await projectService.GetByIdAsync(id);
        if (project is null) return NotFound(new { Success = false, Message = Messages.Project.NotFound });
        return Ok(new { Success = true, Data = project });
    }

    /// <summary>Recherche un projet par son slug.</summary>
    /// <param name="slug">Slug du projet.</param>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var project = await projectService.GetBySlugAsync(slug);
        if (project is null) return NotFound(new { Success = false, Message = Messages.Project.NotFound });
        return Ok(new { Success = true, Data = project });
    }

    /// <summary>Crée un nouveau projet.</summary>
    /// <param name="request">Informations du projet.</param>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var userId = GetUserId();
        var authorName = GetUserName();
        var project = await projectService.CreateAsync(request, userId, authorName);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, new { Success = true, Data = project });
    }

    /// <summary>Modifie un projet existant (auteur ou admin).</summary>
    /// <param name="id">Identifiant du projet.</param>
    /// <param name="request">Nouvelles informations.</param>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var userId = GetUserId();
        var project = await projectService.UpdateAsync(id, request, userId, IsAdmin());
        if (project is null) return NotFound(new { Success = false, Message = Messages.Project.NotFound });
        return Ok(new { Success = true, Data = project });
    }

    /// <summary>Supprime un projet (auteur ou admin).</summary>
    /// <param name="id">Identifiant du projet.</param>
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
