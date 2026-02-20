using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les catégories
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    /// <summary>
    /// Récupérer toutes les catégories
    /// </summary>
    /// <returns>Liste des catégories</returns>
    [HttpGet]
    public IActionResult GetCategories()
    {
        return Ok(new { data = new List<object>() });
    }

    /// <summary>
    /// Récupérer une catégorie par ID
    /// </summary>
    /// <param name="id">ID de la catégorie</param>
    /// <returns>Détails de la catégorie</returns>
    [HttpGet("{id}")]
    public IActionResult GetCategoryById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { message = "ID de la catégorie requis" });

        return NotFound(new { message = "Catégorie non trouvée" });
    }

    /// <summary>
    /// Créer une nouvelle catégorie
    /// </summary>
    /// <param name="request">Données de la catégorie</param>
    /// <returns>Catégorie créée</returns>
    [HttpPost]
    public IActionResult CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Name))
            return BadRequest(new { message = "Nom requis" });

        return CreatedAtAction(nameof(GetCategoryById), new { id = "new-id" }, new { id = "new-id" });
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
