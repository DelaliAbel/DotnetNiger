using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Community;

/// <summary>Contrôleur de gestion des catégories.</summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    /// <summary>Récupère la liste de toutes les catégories.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await categoryService.GetAllAsync();
        return Ok(new { Success = true, Data = categories });
    }

    /// <summary>Récupère une catégorie par son identifiant.</summary>
    [HttpGet("{id:guid}", Order = 1)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var c = await categoryService.GetByIdAsync(id);
        if (c is null) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Data = c });
    }

    /// <summary>Récupère une catégorie par son slug.</summary>
    [HttpGet("{slug}", Order = 2)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var c = await categoryService.GetBySlugAsync(slug);
        if (c is null) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Data = c });
    }

    /// <summary>Crée une nouvelle catégorie.</summary>
    [HttpPost]
    [Authorize(Policy = "community.categories.manage")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var c = await categoryService.CreateAsync(request.Name, request.Description);
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, new { Success = true, Data = c });
    }

    /// <summary>Met à jour une catégorie existante.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "community.categories.manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryRequest request)
    {
        var c = await categoryService.UpdateAsync(id, request.Name, request.Description);
        if (c is null) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Data = c });
    }

    /// <summary>Supprime une catégorie.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "community.categories.manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await categoryService.DeleteAsync(id);
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Message = Messages.Category.Deleted });
    }
}
