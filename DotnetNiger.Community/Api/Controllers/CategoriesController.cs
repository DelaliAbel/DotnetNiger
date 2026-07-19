using Asp.Versioning;
using DotnetNiger.Common.Constants;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des catégories de contenu (posts, ressources, événements).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    /// <summary>Retourne la liste de toutes les catégories.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await categoryService.GetAllAsync();
        return Ok(new { Success = true, Data = categories });
    }

    /// <summary>Recherche une catégorie par son identifiant.</summary>
    /// <param name="id">Identifiant de la catégorie.</param>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var c = await categoryService.GetByIdAsync(id);
        if (c is null) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Data = c });
    }

    /// <summary>Recherche une catégorie par son slug.</summary>
    /// <param name="slug">Slug de la catégorie.</param>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var c = await categoryService.GetBySlugAsync(slug);
        if (c is null) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Data = c });
    }

    /// <summary>Crée une nouvelle catégorie (réservé aux administrateurs).</summary>
    /// <param name="request">Nom et description de la catégorie.</param>
    [HttpPost]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var c = await categoryService.CreateAsync(request.Name, request.Description);
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, new { Success = true, Data = c });
    }

    /// <summary>Met à jour une catégorie existante (réservé aux administrateurs).</summary>
    /// <param name="id">Identifiant de la catégorie.</param>
    /// <param name="request">Nouvelles informations.</param>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryRequest request)
    {
        var c = await categoryService.UpdateAsync(id, request.Name, request.Description);
        if (c is null) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Data = c });
    }

    /// <summary>Supprime une catégorie (réservé aux administrateurs).</summary>
    /// <param name="id">Identifiant de la catégorie.</param>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await categoryService.DeleteAsync(id);
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Message = Messages.Category.Deleted });
    }
}
