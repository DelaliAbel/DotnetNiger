using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Api.Services;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Mappers;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les ressources
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ResourcesController : ApiControllerBase
{
    private readonly IResourceService _resourceService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommunityRequestMapper _requestMapper;

    public ResourcesController(
        IResourceService resourceService,
        ICurrentUserService currentUserService,
        ICommunityRequestMapper requestMapper)
    {
        _resourceService = resourceService;
        _currentUserService = currentUserService;
        _requestMapper = requestMapper;
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
            return BadRequestProblem("Parametres de pagination invalides");

        var resources = await _resourceService.GetAllResourcesAsync(page, pageSize);
        return Success(resources, meta: new { page, pageSize, total = resources.Count() });
    }

    /// <summary>
    /// Récupérer une ressource par ID
    /// </summary>
    /// <param name="id">ID de la ressource</param>
    /// <returns>Détails de la ressource</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetResourceById(string id)
    {
        var resourceId = ParseGuidOrThrow(id, nameof(id), "ID de la ressource invalide");

        var resource = await _resourceService.GetResourceByIdAsync(resourceId);
        if (resource == null)
            return NotFoundProblem("Ressource non trouvee");

        return Success(resource);
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
            return BadRequestProblem("Titre requis");

        var currentUserId = _currentUserService.GetRequiredUserId();
        var resource = _requestMapper.MapToResource(request, currentUserId);

        var createdResource = await _resourceService.CreateResourceAsync(resource);
        return CreatedSuccess(nameof(GetResourceById), new { id = createdResource.Id }, createdResource, "Ressource creee avec succes");
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
        var resourceId = ParseGuidOrThrow(id, nameof(id), "ID de la ressource invalide");

        var resource = await _resourceService.GetResourceByIdAsync(resourceId);
        if (resource == null)
            return NotFoundProblem("Ressource non trouvee");

        _requestMapper.ApplyResourceUpdates(resource, request);
        var updatedResource = await _resourceService.UpdateResourceAsync(resource);
        return Success(updatedResource, "Ressource mise a jour avec succes");
    }

    /// <summary>
    /// Supprimer une ressource
    /// </summary>
    /// <param name="id">ID de la ressource</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResource(string id)
    {
        var resourceId = ParseGuidOrThrow(id, nameof(id), "ID de la ressource invalide");

        var deleted = await _resourceService.DeleteResourceAsync(resourceId);
        if (!deleted)
            return NotFoundProblem("Ressource non trouvee");

        return SuccessMessage("Ressource supprimee avec succes");
    }
}

