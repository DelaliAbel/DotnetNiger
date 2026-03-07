using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les projets
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Récupérer tous les projets
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Nombre de projets par page (par défaut 10)</param>
    /// <returns>Liste paginée des projets</returns>
    [HttpGet]
    public async Task<IActionResult> GetProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Paramètres de pagination invalides" });

        try
        {
            var projects = await _projectService.GetAllProjectsAsync(page, pageSize);
            return Ok(new
            {
                page = page,
                pageSize = pageSize,
                total = projects.Count(),
                data = projects
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des projets", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupérer un projet par ID
    /// </summary>
    /// <param name="id">ID du projet</param>
    /// <returns>Détails du projet</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProjectById(string id)
    {
        if (!Guid.TryParse(id, out var projectId))
            return BadRequest(new { message = "ID du projet invalide" });

        try
        {
            var project = await _projectService.GetProjectByIdAsync(projectId);
            if (project == null)
                return NotFound(new { message = "Projet non trouvé" });

            return Ok(project);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération du projet", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupérer les projets actifs
    /// </summary>
    /// <returns>Projets actifs</returns>
    [HttpGet("active/list")]
    public async Task<IActionResult> GetActiveProjects()
    {
        try
        {
            var projects = await _projectService.GetActiveProjectsAsync();
            return Ok(new { data = projects });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des projets actifs", error = ex.Message });
        }
    }

    /// <summary>
    /// Créer un nouveau projet
    /// </summary>
    /// <param name="request">Données du projet</param>
    /// <returns>Projet créé</returns>
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Name))
            return BadRequest(new { message = "Nom du projet requis" });

        if (!this.TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(new { message = "Utilisateur non authentifie", details = "Claim user ou header X-User-Id requis" });

        try
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = request.Name.ToLower().Replace(" ", "-"),
                Description = request.Description ?? string.Empty,
                GitHubUrl = request.GitHubUrl ?? string.Empty,
                OwnerId = currentUserId,
                IsFeatured = false,
                Stars = 0,
                ContributorsCount = 0,
                Language = request.Language ?? "C#",
                License = request.License ?? "MIT",
                CreatedAt = DateTime.UtcNow
            };

            var createdProject = await _projectService.CreateProjectAsync(project);
            return CreatedAtAction(nameof(GetProjectById), new { id = createdProject.Id }, createdProject);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la création du projet", error = ex.Message });
        }
    }

    /// <summary>
    /// Mettre à jour un projet
    /// </summary>
    /// <param name="id">ID du projet</param>
    /// <param name="request">Données à mettre à jour</param>
    /// <returns>Projet mis à jour</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(string id, [FromBody] UpdateProjectRequest request)
    {
        if (!Guid.TryParse(id, out var projectId))
            return BadRequest(new { message = "ID du projet invalide" });

        try
        {
            var project = await _projectService.GetProjectByIdAsync(projectId);
            if (project == null)
                return NotFound(new { message = "Projet non trouvé" });

            if (!string.IsNullOrEmpty(request.Name))
                project.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description))
                project.Description = request.Description;
            if (!string.IsNullOrEmpty(request.GitHubUrl))
                project.GitHubUrl = request.GitHubUrl;

            var updatedProject = await _projectService.UpdateProjectAsync(project);
            return Ok(updatedProject);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la mise à jour du projet", error = ex.Message });
        }
    }

    /// <summary>
    /// Supprimer un projet
    /// </summary>
    /// <param name="id">ID du projet</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(string id)
    {
        if (!Guid.TryParse(id, out var projectId))
            return BadRequest(new { message = "ID du projet invalide" });

        try
        {
            var deleted = await _projectService.DeleteProjectAsync(projectId);
            if (!deleted)
                return NotFound(new { message = "Projet non trouvé" });

            return Ok(new { message = "Projet supprimé avec succès" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la suppression du projet", error = ex.Message });
        }
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
    /// <summary>GitHub URL</summary>
    public string? GitHubUrl { get; set; }
    /// <summary>Langage programming</summary>
    public string? Language { get; set; }
    /// <summary>License</summary>
    public string? License { get; set; }
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
    /// <summary>GitHub URL</summary>
    public string? GitHubUrl { get; set; }
}
