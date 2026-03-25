using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les catégories
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Récupérer toutes les catégories
    /// </summary>
    /// <returns>Liste des catégories</returns>
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(new { data = categories });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des catégories", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupérer une catégorie par ID
    /// </summary>
    /// <param name="id">ID de la catégorie</param>
    /// <returns>Détails de la catégorie</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(string id)
    {
        if (!Guid.TryParse(id, out var categoryId))
            return BadRequest(new { message = "ID de la catégorie invalide" });

        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
                return NotFound(new { message = "Catégorie non trouvée" });

            return Ok(category);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération de la catégorie", error = ex.Message });
        }
    }

    /// <summary>
    /// Créer une nouvelle catégorie
    /// </summary>
    /// <param name="request">Données de la catégorie</param>
    /// <returns>Catégorie créée</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Name))
            return BadRequest(new { message = "Nom requis" });

        try
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = request.Name.ToLower().Replace(" ", "-"),
                Description = request.Description ?? string.Empty
            };

            var createdCategory = await _categoryService.CreateCategoryAsync(category);
            return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la création de la catégorie", error = ex.Message });
        }
    }

    /// <summary>
    /// Mettre à jour une catégorie
    /// </summary>
    /// <param name="id">ID de la catégorie</param>
    /// <param name="request">Données à mettre à jour</param>
    /// <returns>Catégorie mise à jour</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(string id, [FromBody] UpdateCategoryRequest request)
    {
        if (!Guid.TryParse(id, out var categoryId))
            return BadRequest(new { message = "ID de la catégorie invalide" });

        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
                return NotFound(new { message = "Catégorie non trouvée" });

            if (!string.IsNullOrEmpty(request.Name))
                category.Name = request.Name;
            if (request.Description != null)
                category.Description = request.Description;

            var updatedCategory = await _categoryService.UpdateCategoryAsync(category);
            return Ok(updatedCategory);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la mise à jour de la catégorie", error = ex.Message });
        }
    }

    /// <summary>
    /// Supprimer une catégorie
    /// </summary>
    /// <param name="id">ID de la catégorie</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        if (!Guid.TryParse(id, out var categoryId))
            return BadRequest(new { message = "ID de la catégorie invalide" });

        try
        {
            var deleted = await _categoryService.DeleteCategoryAsync(categoryId);
            if (!deleted)
                return NotFound(new { message = "Catégorie non trouvée" });

            return Ok(new { message = "Catégorie supprimée avec succès" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la suppression de la catégorie", error = ex.Message });
        }
    }
}

/// <summary>
/// DTO pour créer une catégorie
/// </summary>
public class CreateCategoryRequest
{
    /// <summary>Nom de la catégorie</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Description de la catégorie</summary>
    public string? Description { get; set; }
}

/// <summary>
/// DTO pour mettre à jour une catégorie
/// </summary>
public class UpdateCategoryRequest
{
    /// <summary>Nom de la catégorie</summary>
    public string? Name { get; set; }
    /// <summary>Description de la catégorie</summary>
    public string? Description { get; set; }
}
