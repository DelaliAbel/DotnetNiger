using Asp.Versioning;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using DotnetNiger.Community.Application;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des tags utilisés pour catégoriser le contenu.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TagsController(ITagService tagService) : ControllerBase
{
    /// <summary>Retourne la liste de tous les tags.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tags = await tagService.GetAllAsync();
        return Ok(new { Success = true, Data = tags });
    }

    /// <summary>Recherche un tag par son identifiant.</summary>
    /// <param name="id">Identifiant du tag.</param>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var t = await tagService.GetByIdAsync(id);
        if (t is null) return NotFound(new { Success = false, Message = Messages.Tag.NotFound });
        return Ok(new { Success = true, Data = t });
    }

    /// <summary>Recherche un tag par son slug.</summary>
    /// <param name="slug">Slug du tag.</param>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var t = await tagService.GetBySlugAsync(slug);
        if (t is null) return NotFound(new { Success = false, Message = Messages.Tag.NotFound });
        return Ok(new { Success = true, Data = t });
    }

    /// <summary>Crée un nouveau tag (réservé aux administrateurs).</summary>
    /// <param name="request">Nom du tag.</param>
    [HttpPost]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
    {
        var t = await tagService.CreateAsync(request.Name);
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, new { Success = true, Data = t });
    }

    /// <summary>Modifie un tag existant (réservé aux administrateurs).</summary>
    /// <param name="id">Identifiant du tag.</param>
    /// <param name="request">Nouveau nom.</param>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateTagRequest request)
    {
        var t = await tagService.UpdateAsync(id, request.Name);
        if (t is null) return NotFound(new { Success = false, Message = Messages.Tag.NotFound });
        return Ok(new { Success = true, Data = t });
    }

    /// <summary>Supprime un tag (réservé aux administrateurs).</summary>
    /// <param name="id">Identifiant du tag.</param>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await tagService.DeleteAsync(id);
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Tag.NotFound });
        return Ok(new { Success = true, Message = Messages.Tag.Deleted });
    }
}
