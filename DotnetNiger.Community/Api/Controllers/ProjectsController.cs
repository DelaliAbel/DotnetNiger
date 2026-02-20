using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les projets
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    /// <summary>
    /// Récupérer tous les projets
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Nombre de projets par page (par défaut 10)</param>
    /// <returns>Liste paginée des projets</returns>
    [HttpGet]
    public IActionResult GetProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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
    /// Récupérer un projet par ID
    /// </summary>
    /// <param name="id">ID du projet</param>
    /// <returns>Détails du projet</returns>
    [HttpGet("{id}")]
    public IActionResult GetProjectById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { message = "ID du projet requis" });

        return NotFound(new { message = "Projet non trouvé" });
    }

    /// <summary>
    /// Créer un nouveau projet
    /// </summary>
    /// <param name="request">Données du projet</param>
    /// <returns>Projet créé</returns>
    [HttpPost]
    public IActionResult CreateProject([FromBody] CreateProjectRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Name))
            return BadRequest(new { message = "Nom du projet requis" });

        return CreatedAtAction(nameof(GetProjectById), new { id = "new-id" }, new { id = "new-id" });
    }

    /// <summary>
    /// Mettre à jour un projet
    /// </summary>
    /// <param name="id">ID du projet</param>
    /// <param name="request">Données à mettre à jour</param>
    /// <returns>Projet mis à jour</returns>
    [HttpPut("{id}")]
    public IActionResult UpdateProject(string id, [FromBody] UpdateProjectRequest request)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { message = "ID du projet requis" });

        return Ok(new { message = "Projet mis à jour" });
    }
}

/// <summary>
/// DTO pour créer un projet
/// </summary>
public class CreateProjectRequest
{
    /// <summary>Nom du projet</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Description du projet</summary>
    public string? Description { get; set; }
    /// <summary>Repository URL</summary>
    public string? RepositoryUrl { get; set; }
    /// <summary>Tags du projet</summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// DTO pour mettre à jour un projet
/// </summary>
public class UpdateProjectRequest
{
    /// <summary>Nom du projet</summary>
    public string? Name { get; set; }
    /// <summary>Description du projet</summary>
    public string? Description { get; set; }
}
