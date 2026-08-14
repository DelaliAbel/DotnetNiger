using System.Threading;
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
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var tags = await tagService.GetAllAsync(ct);
        return Success(tags);
    }

    /// <summary>Récupère un tag par son identifiant.</summary>
    [HttpGet("{id:guid}", Order = 1)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var t = await tagService.GetByIdAsync(id, ct);
        if (t is null) return NotFound(Messages.Tag.NotFound);
        return Success(t);
    }

    /// <summary>Récupère un tag par son slug.</summary>
    [HttpGet("{slug}", Order = 2)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct = default)
    {
        var t = await tagService.GetBySlugAsync(slug, ct);
        if (t is null) return NotFound(Messages.Tag.NotFound);
        return Success(t);
    }

    /// <summary>Crée un nouveau tag.</summary>
    [HttpPost]
    [Authorize(Policy = "community.tags.manage")]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request, CancellationToken ct = default)
    {
        var t = await tagService.CreateAsync(request.Name, ct);
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, new { success = true, data = t, message = (string?)null });
    }

    /// <summary>Met à jour un tag existant.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "community.tags.manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateTagRequest request, CancellationToken ct = default)
    {
        var t = await tagService.UpdateAsync(id, request.Name, ct);
        if (t is null) return NotFound(Messages.Tag.NotFound);
        return Success(t);
    }

    /// <summary>Supprime un tag.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "community.tags.manage")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var deleted = await tagService.DeleteAsync(id, ct);
        if (!deleted) return NotFound(Messages.Tag.NotFound);
        return Success<object?>(null, Messages.Tag.Deleted);
    }
}
