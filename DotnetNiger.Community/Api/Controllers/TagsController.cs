using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les tags
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    /// <summary>
    /// Récupérer tous les tags
    /// </summary>
    /// <returns>Liste des tags</returns>
    [HttpGet]
    public IActionResult GetTags()
    {
        return Ok(new { data = new List<string>() });
    }

    /// <summary>
    /// Récupérer un tag par ID
    /// </summary>
    /// <param name="id">ID du tag</param>
    /// <returns>Détails du tag</returns>
    [HttpGet("{id}")]
    public IActionResult GetTagById(string id)
    {
        return NotFound(new { message = "Tag non trouvé" });
    }

    /// <summary>
    /// Créer un nouveau tag
    /// </summary>
    /// <param name="request">Données du tag</param>
    /// <returns>Tag créé</returns>
    [HttpPost]
    public IActionResult CreateTag([FromBody] CreateTagRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Name))
            return BadRequest(new { message = "Nom du tag requis" });

        return CreatedAtAction(nameof(GetTagById), new { id = "new-id" }, new { id = "new-id" });
    }
}

/// <summary>
/// DTO pour créer un tag
/// </summary>
public class CreateTagRequest
{
    /// <summary>Nom du tag</summary>
    public string Name { get; set; } = string.Empty;
}
