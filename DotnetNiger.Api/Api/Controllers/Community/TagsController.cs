using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Community;

/// <summary>Contrôleur de gestion des tags.</summary>
[ApiController]
[Route("api/tags")]
public class TagsController(ITagService tagService) : BaseController
{
    /// <summary>Récupère la liste de tous les tags.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tags = await tagService.GetAllAsync();
        return Success(tags);
    }

    /// <summary>Récupère un tag par son identifiant.</summary>
    [HttpGet("{id:guid}", Order = 1)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var t = await tagService.GetByIdAsync(id);
        if (t is null) return NotFound(Messages.Tag.NotFound);
        return Success(t);
    }

    /// <summary>Récupère un tag par son slug.</summary>
    [HttpGet("{slug}", Order = 2)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var t = await tagService.GetBySlugAsync(slug);
        if (t is null) return NotFound(Messages.Tag.NotFound);
        return Success(t);
    }

    /// <summary>Crée un nouveau tag.</summary>
    [HttpPost]
    [Authorize(Policy = "community.tags.manage")]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
    {
        var t = await tagService.CreateAsync(request.Name);
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, new { success = true, data = t, message = (string?)null });
    }

    /// <summary>Met à jour un tag existant.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "community.tags.manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateTagRequest request)
    {
        var t = await tagService.UpdateAsync(id, request.Name);
        if (t is null) return NotFound(Messages.Tag.NotFound);
        return Success(t);
    }

    /// <summary>Supprime un tag.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "community.tags.manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await tagService.DeleteAsync(id);
        if (!deleted) return NotFound(Messages.Tag.NotFound);
        return Success<object?>(null, Messages.Tag.Deleted);
    }
}
