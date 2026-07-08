using Asp.Versioning;
using DotnetNiger.Community.Application;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Common.Constants;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des ressources pédagogiques (vidéos, documents, templates, ebooks...).</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ResourcesController(IResourceQueryService resourceQuery, IResourceCommandService resourceCommand) : BaseController
{
    /// <summary>Recherche et filtre les ressources avec pagination.</summary>
    /// <param name="resourceType">Type de ressource.</param>
    /// <param name="level">Niveau (débutant, intermédiaire, avancé).</param>
    /// <param name="query">Recherche textuelle.</param>
    /// <param name="tag">Filtre par tag.</param>
    /// <param name="categoryId">Filtre par catégorie.</param>
    /// <param name="page">Page demandée.</param>
    /// <param name="pageSize">Taille de la page.</param>
    /// <param name="after">Curseur pour la pagination.</param>
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

    /// <summary>Retourne les ressources créées par l'utilisateur courant.</summary>
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

    /// <summary>Recherche une ressource par son identifiant.</summary>
    /// <param name="id">Identifiant de la ressource.</param>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var resource = await resourceQuery.GetByIdAsync(id);
        if (resource is null) return NotFound(new { Success = false, Message = Messages.Resource.NotFound });
        return Ok(new { Success = true, Data = resource });
    }

    /// <summary>Recherche une ressource par son slug.</summary>
    /// <param name="slug">Slug de la ressource.</param>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var resource = await resourceQuery.GetBySlugAsync(slug);
        if (resource is null) return NotFound(new { Success = false, Message = Messages.Resource.NotFound });
        return Ok(new { Success = true, Data = resource });
    }

    /// <summary>Récupère les métadonnées Open Graph d'une ressource pour le partage social.</summary>
    /// <param name="slug">Slug de la ressource.</param>
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

    /// <summary>Retourne la liste des types de ressources disponibles.</summary>
    [HttpGet("types")]
    public async Task<IActionResult> GetTypes()
    {
        var types = await resourceQuery.GetResourceTypesAsync();
        return Ok(new { Success = true, Data = types });
    }

    /// <summary>Retourne la liste des niveaux disponibles (débutant, intermédiaire, avancé).</summary>
    [HttpGet("levels")]
    public async Task<IActionResult> GetLevels()
    {
        var levels = await resourceQuery.GetLevelsAsync();
        return Ok(new { Success = true, Data = levels });
    }

    /// <summary>Crée une nouvelle ressource. Les collaborateurs doivent avoir un certificat validé.</summary>
    /// <param name="request">Informations de la ressource.</param>
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

    /// <summary>Modifie une ressource existante (auteur ou admin).</summary>
    /// <param name="id">Identifiant de la ressource.</param>
    /// <param name="request">Nouvelles informations.</param>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateResourceRequest request)
    {
        var userId = GetUserId();
        var resource = await resourceCommand.UpdateAsync(id, request, userId, IsAdmin());
        if (resource is null) return NotFound(new { Success = false, Message = Messages.Resource.NotFound });
        return Ok(new { Success = true, Data = resource });
    }

    /// <summary>Supprime une ressource (auteur ou admin).</summary>
    /// <param name="id">Identifiant de la ressource.</param>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = await resourceCommand.DeleteAsync(id, userId, IsAdmin());
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Resource.NotFound });
        return Ok(new { Success = true, Message = Messages.Resource.Deleted });
    }

    /// <summary>Incémente le compteur de vues d'une ressource.</summary>
    /// <param name="id">Identifiant de la ressource.</param>
    [HttpPost("{id:guid}/views")]
    public async Task<IActionResult> IncrementViewCount(Guid id)
    {
        var resource = await resourceCommand.IncrementViewCountAsync(id);
        if (resource is null) return NotFound(new { Success = false, Message = Messages.Resource.NotFound });
        return Ok(new { Success = true, Data = resource });
    }
}
