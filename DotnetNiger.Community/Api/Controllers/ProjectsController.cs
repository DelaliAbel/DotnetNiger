using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Api.Services;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Mappers;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les projets
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProjectsController : ApiControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommunityRequestMapper _requestMapper;

    public ProjectsController(
        IProjectService projectService,
        ICurrentUserService currentUserService,
        ICommunityRequestMapper requestMapper)
    {
        _projectService = projectService;
        _currentUserService = currentUserService;
        _requestMapper = requestMapper;
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
            return BadRequestProblem("Parametres de pagination invalides");

        var projects = await _projectService.GetAllProjectsAsync(page, pageSize);
        return Success(projects, meta: new { page, pageSize, total = projects.Count() });
    }

    /// <summary>
    /// Récupérer un projet par ID
    /// </summary>
    /// <param name="id">ID du projet</param>
    /// <returns>Détails du projet</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProjectById(string id)
    {
        var projectId = ParseGuidOrThrow(id, nameof(id), "ID du projet invalide");

        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
            return NotFoundProblem("Projet non trouve");

        return Success(project);
    }

    /// <summary>
    /// Récupérer les projets actifs
    /// </summary>
    /// <returns>Projets actifs</returns>
    [HttpGet("active/list")]
    public async Task<IActionResult> GetActiveProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequestProblem("Invalid pagination parameters");

        var projects = await _projectService.GetActiveProjectsAsync();
        var paginatedProjects = projects.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Success(paginatedProjects, meta: new { page, pageSize, total = projects.Count() });
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
            return BadRequestProblem("Nom du projet requis");

        var currentUserId = _currentUserService.GetRequiredUserId();
        var project = _requestMapper.MapToProject(request, currentUserId);

        var createdProject = await _projectService.CreateProjectAsync(project);
        return CreatedSuccess(nameof(GetProjectById), new { id = createdProject.Id }, createdProject, "Projet cree avec succes");
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
        var projectId = ParseGuidOrThrow(id, nameof(id), "ID du projet invalide");

        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
            return NotFoundProblem("Projet non trouve");

        _requestMapper.ApplyProjectUpdates(project, request);
        var updatedProject = await _projectService.UpdateProjectAsync(project);
        return Success(updatedProject, "Projet mis a jour avec succes");
    }

    /// <summary>
    /// Supprimer un projet
    /// </summary>
    /// <param name="id">ID du projet</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(string id)
    {
        var projectId = ParseGuidOrThrow(id, nameof(id), "ID du projet invalide");

        var deleted = await _projectService.DeleteProjectAsync(projectId);
        if (!deleted)
            return NotFoundProblem("Projet non trouve");

        return SuccessMessage("Projet supprime avec succes");
    }

}


