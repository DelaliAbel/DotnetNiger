using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Mappers;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les catégories
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ICommunityRequestMapper _requestMapper;

    public CategoriesController(ICategoryService categoryService, ICommunityRequestMapper requestMapper)
    {
        _categoryService = categoryService;
        _requestMapper = requestMapper;
    }

    /// <summary>
    /// Récupérer toutes les catégories
    /// </summary>
    /// <returns>Liste des catégories</returns>
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Success(categories);
    }

    /// <summary>
    /// Récupérer une catégorie par ID
    /// </summary>
    /// <param name="id">ID de la catégorie</param>
    /// <returns>Détails de la catégorie</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(string id)
    {
        var categoryId = ParseGuidOrThrow(id, nameof(id), "ID de la categorie invalide");

        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
            return NotFoundProblem("Categorie non trouvee");

        return Success(category);
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
            return BadRequestProblem("Nom requis");

        var category = _requestMapper.MapToCategory(request);
        var createdCategory = await _categoryService.CreateCategoryAsync(category);
        return CreatedSuccess(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory, "Categorie creee avec succes");
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
        var categoryId = ParseGuidOrThrow(id, nameof(id), "ID de la categorie invalide");

        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
            return NotFoundProblem("Categorie non trouvee");

        _requestMapper.ApplyCategoryUpdates(category, request);
        var updatedCategory = await _categoryService.UpdateCategoryAsync(category);
        return Success(updatedCategory, "Categorie mise a jour avec succes");
    }

    /// <summary>
    /// Supprimer une catégorie
    /// </summary>
    /// <param name="id">ID de la catégorie</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        var categoryId = ParseGuidOrThrow(id, nameof(id), "ID de la categorie invalide");

        var deleted = await _categoryService.DeleteCategoryAsync(categoryId);
        if (!deleted)
            return NotFoundProblem("Categorie non trouvee");

        return SuccessMessage("Categorie supprimee avec succes");
    }
}

