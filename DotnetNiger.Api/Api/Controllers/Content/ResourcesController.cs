using System.Threading;
using DotnetNiger.Api.Application.Interfaces;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Content;

/// <summary>Contrôleur de gestion des ressources éducatives.</summary>
[ApiController]
[Route("api/resources")]
public class ResourcesController(IResourceQueryService resourceQuery, IResourceCommandService resourceCommand) : BaseController
{
    /// <summary>Récupère la liste paginée des ressources avec filtres optionnels.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? resourceType, [FromQuery] string? level, [FromQuery] string? query,
        [FromQuery] string? tag, [FromQuery] Guid? categoryId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] Guid? after = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await resourceQuery.GetAllAsync(resourceType, level, query, tag, categoryId, page, pageSize, after, ct: ct);
        return Success(result);
    }

    /// <summary>Récupère les ressources de l'utilisateur connecté.</summary>
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var userId = GetUserId();
        var result = await resourceQuery.GetAllAsync(null, null, null, null, null, page, pageSize, null, userId, ct);
        return Success(result);
    }

    /// <summary>Récupère une ressource par son identifiant.</summary>
    [HttpGet("{id:guid}", Order = 1)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var resource = await resourceQuery.GetByIdAsync(id, ct);
        if (resource is null) return NotFound(Messages.Resource.NotFound);
        return Success(resource);
    }

    /// <summary>Récupère une ressource par son slug.</summary>
    [HttpGet("{slug}", Order = 2)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct = default)
    {
        var resource = await resourceQuery.GetBySlugAsync(slug, ct);
        if (resource is null) return NotFound(Messages.Resource.NotFound);
        return Success(resource);
    }

    /// <summary>Récupère les métadonnées Open Graph d'une ressource par son slug.</summary>
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetOGBySlug(string slug, CancellationToken ct = default)
    {
        var resource = await resourceQuery.GetBySlugAsync(slug, ct);
        if (resource is null) return NotFound(Messages.Resource.NotFound);

        return Success(new OGMetadata
        {
            Title = resource.Title,
            Description = resource.Description,
            ImageUrl = resource.ThumbnailUrl ?? string.Empty,
            UpdatedAt = resource.UpdatedAt
        });
    }

    /// <summary>Récupère la liste des types de ressources disponibles.</summary>
    [HttpGet("types")]
    public async Task<IActionResult> GetTypes(CancellationToken ct = default)
    {
        var types = await resourceQuery.GetResourceTypesAsync(ct);
        return Success(types);
    }

    /// <summary>Récupère la liste des niveaux de difficulté disponibles.</summary>
    [HttpGet("levels")]
    public async Task<IActionResult> GetLevels(CancellationToken ct = default)
    {
        var levels = await resourceQuery.GetLevelsAsync(ct);
        return Success(levels);
    }

    /// <summary>Crée une nouvelle ressource.</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateResourceRequest request, CancellationToken ct = default)
    {
        var userId = GetUserId();
        try
        {
            var resource = await resourceCommand.CreateAsync(request, userId, IsAdmin(), IsCollaborator(), ct);
            return CreatedAtAction(nameof(GetById), new { id = resource.Id }, new { success = true, data = resource, message = (string?)null });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Met à jour une ressource existante.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResourceRequest request, CancellationToken ct = default)
    {
        try
        {
            var userId = GetUserId();
            var resource = await resourceCommand.UpdateAsync(id, request, userId, IsAdmin(), ct);
            if (resource is null) return NotFound(Messages.Resource.NotFound);
            return Success(resource);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, 403);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Supprime une ressource.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        try
        {
            var userId = GetUserId();
            var deleted = await resourceCommand.DeleteAsync(id, userId, IsAdmin(), ct);
            if (!deleted) return NotFound(Messages.Resource.NotFound);
            return Success<object?>(null, Messages.Resource.Deleted);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, 403);
        }
    }

    /// <summary>Incrémente le compteur de vues d'une ressource.</summary>
    [HttpPost("{id:guid}/views")]
    public async Task<IActionResult> IncrementViewCount(Guid id, CancellationToken ct = default)
    {
        var resource = await resourceCommand.IncrementViewCountAsync(id, ct);
        if (resource is null) return NotFound(Messages.Resource.NotFound);
        return Success(resource);
    }
}
