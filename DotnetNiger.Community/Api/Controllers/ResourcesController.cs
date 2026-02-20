using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les ressources
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    /// <summary>
    /// Récupérer toutes les ressources
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Nombre de ressources par page (par défaut 10)</param>
    /// <returns>Liste paginée des ressources</returns>
    [HttpGet]
    public IActionResult GetResources([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Paramètres de pagination invalides" });

        return Ok(new
        {
            page = page,
            pageSize = pageSize,
            total = 0,
            data = new List<object>()
        });
    }

    /// <summary>
    /// Récupérer une ressource par ID
    /// </summary>
    /// <param name="id">ID de la ressource</param>
    /// <returns>Détails de la ressource</returns>
    [HttpGet("{id}")]
    public IActionResult GetResourceById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { message = "ID de la ressource requis" });

        return NotFound(new { message = "Ressource non trouvée" });
    }

    /// <summary>
    /// Créer une nouvelle ressource
    /// </summary>
    /// <param name="request">Données de la ressource</param>
    /// <returns>Ressource créée</returns>
    [HttpPost]
    public IActionResult CreateResource([FromBody] CreateResourceRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Title))
            return BadRequest(new { message = "Titre requis" });

        return CreatedAtAction(nameof(GetResourceById), new { id = "new-id" }, new { id = "new-id" });
    }

    /// <summary>
    /// Mettre à jour une ressource
    /// </summary>
    /// <param name="id">ID de la ressource</param>
    /// <param name="request">Données à mettre à jour</param>
    /// <returns>Ressource mise à jour</returns>
    [HttpPut("{id}")]
    public IActionResult UpdateResource(string id, [FromBody] UpdateResourceRequest request)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { message = "ID de la ressource requis" });

        return Ok(new { message = "Ressource mise à jour" });
    }
}

/// <summary>
/// DTO pour créer une ressource
/// </summary>
public class CreateResourceRequest
{
    /// <summary>Titre de la ressource</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Description de la ressource</summary>
    public string? Description { get; set; }
    /// <summary>URL de la ressource</summary>
    public string? Url { get; set; }
    /// <summary>Type de ressource</summary>
    public string? ResourceType { get; set; }
}

/// <summary>
/// DTO pour mettre à jour une ressource
/// </summary>
public class UpdateResourceRequest
{
    /// <summary>Titre de la ressource</summary>
    public string? Title { get; set; }
    /// <summary>Description de la ressource</summary>
    public string? Description { get; set; }
}
