using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les ressources
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;

    public ResourcesController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    /// <summary>
    /// Récupérer toutes les ressources
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Nombre de ressources par page (par défaut 10)</param>
    /// <returns>Liste paginée des ressources</returns>
    [HttpGet]
    public async Task<IActionResult> GetResources([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Paramètres de pagination invalides" });

        try
        {
            var resources = await _resourceService.GetAllResourcesAsync(page, pageSize);
            return Ok(new
            {
                page = page,
                pageSize = pageSize,
                total = resources.Count(),
                data = resources
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des ressources", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupérer une ressource par ID
    /// </summary>
    /// <param name="id">ID de la ressource</param>
    /// <returns>Détails de la ressource</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetResourceById(string id)
    {
        if (!Guid.TryParse(id, out var resourceId))
            return BadRequest(new { message = "ID de la ressource invalide" });

        try
        {
            var resource = await _resourceService.GetResourceByIdAsync(resourceId);
            if (resource == null)
                return NotFound(new { message = "Ressource non trouvée" });

            return Ok(resource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération de la ressource", error = ex.Message });
        }
    }

    /// <summary>
    /// Créer une nouvelle ressource
    /// </summary>
    /// <param name="request">Données de la ressource</param>
    /// <returns>Ressource créée</returns>
    [HttpPost]
    public async Task<IActionResult> CreateResource([FromBody] CreateResourceRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Title))
            return BadRequest(new { message = "Titre requis" });

        if (!this.TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(new { message = "Utilisateur non authentifie", details = "Claim user ou header X-User-Id requis" });

        try
        {
            var resource = new Resource
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Slug = request.Title.ToLower().Replace(" ", "-"),
                Description = request.Description ?? string.Empty,
                Url = request.Url ?? string.Empty,
                ResourceType = request.ResourceType ?? "Documentation",
                Level = request.Level ?? "Beginner",
                CreatedBy = currentUserId,
                IsApproved = false,
                ViewCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            var createdResource = await _resourceService.CreateResourceAsync(resource);
            return CreatedAtAction(nameof(GetResourceById), new { id = createdResource.Id }, createdResource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la création de la ressource", error = ex.Message });
        }
    }

    /// <summary>
    /// Mettre à jour une ressource
    /// </summary>
    /// <param name="id">ID de la ressource</param>
    /// <param name="request">Données à mettre à jour</param>
    /// <returns>Ressource mise à jour</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResource(string id, [FromBody] UpdateResourceRequest request)
    {
        if (!Guid.TryParse(id, out var resourceId))
            return BadRequest(new { message = "ID de la ressource invalide" });

        try
        {
            var resource = await _resourceService.GetResourceByIdAsync(resourceId);
            if (resource == null)
                return NotFound(new { message = "Ressource non trouvée" });

            if (!string.IsNullOrEmpty(request.Title))
                resource.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Description))
                resource.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Url))
                resource.Url = request.Url;

            var updatedResource = await _resourceService.UpdateResourceAsync(resource);
            return Ok(updatedResource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la mise à jour de la ressource", error = ex.Message });
        }
    }

    /// <summary>
    /// Supprimer une ressource
    /// </summary>
    /// <param name="id">ID de la ressource</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResource(string id)
    {
        if (!Guid.TryParse(id, out var resourceId))
            return BadRequest(new { message = "ID de la ressource invalide" });

        try
        {
            var deleted = await _resourceService.DeleteResourceAsync(resourceId);
            if (!deleted)
                return NotFound(new { message = "Ressource non trouvée" });

            return Ok(new { message = "Ressource supprimée avec succès" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la suppression de la ressource", error = ex.Message });
        }
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
    /// <summary>Niveau de difficulté</summary>
    public string? Level { get; set; }
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
    /// <summary>URL de la ressource</summary>
    public string? Url { get; set; }
}
