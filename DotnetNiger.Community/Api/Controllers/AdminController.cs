using Asp.Versioning;
using DotnetNiger.Community.Api.Filters;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Endpoints dédiés à l'administration de la communauté.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[AuthorizeFilter("admin", "super-admin", "moderator")]
public class AdminController : ApiControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ─── Dashboard ────────────────────────────────────────────────────────────

    /// <summary>Tableau de bord admin : totaux, modération, publication.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _adminService.GetDashboardAsync();
        return Success(dashboard);
    }

    // ─── Resources ────────────────────────────────────────────────────────────

    /// <summary>Ressources en attente de modération.</summary>
    [HttpGet("resources")]
    public async Task<IActionResult> GetPendingResources([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequestProblem("Parametres de pagination invalides");

        var resources = await _adminService.GetPendingResourcesAsync(page, pageSize);
        return Success(resources, meta: new { page, pageSize });
    }

    /// <summary>Approuver ou rejeter une ressource.</summary>
    [HttpPatch("resources/{id}")]
    public async Task<IActionResult> ModerateResource(string id, [FromBody] ModerateResourceRequest request)
    {
        var resourceId = ParseGuidOrThrow(id, nameof(id), "ID de la ressource invalide");

        var updated = await _adminService.ModerateResourceAsync(resourceId, request.IsApproved);
        if (!updated)
            return NotFoundProblem("Ressource non trouvee");

        return SuccessMessage(request.IsApproved ? "Ressource approuvee" : "Ressource rejetee et supprimee");
    }

    // ─── Posts ────────────────────────────────────────────────────────────────

    /// <summary>Tous les posts (publiés et brouillons) avec pagination.</summary>
    [HttpGet("posts")]
    public async Task<IActionResult> GetAllPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequestProblem("Parametres de pagination invalides");

        var posts = await _adminService.GetAllPostsAsync(page, pageSize);
        return Success(posts, meta: new { page, pageSize });
    }

    /// <summary>Publier un post.</summary>
    [HttpPatch("posts/{id}/publish")]
    public async Task<IActionResult> PublishPost(string id)
    {
        var postId = ParseGuidOrThrow(id, nameof(id), "ID du post invalide");

        var ok = await _adminService.PublishPostAsync(postId);
        if (!ok)
            return NotFoundProblem("Post non trouve");

        return SuccessMessage("Post publie");
    }

    /// <summary>Dépublier un post.</summary>
    [HttpPatch("posts/{id}/unpublish")]
    public async Task<IActionResult> UnpublishPost(string id)
    {
        var postId = ParseGuidOrThrow(id, nameof(id), "ID du post invalide");

        var ok = await _adminService.UnpublishPostAsync(postId);
        if (!ok)
            return NotFoundProblem("Post non trouve");

        return SuccessMessage("Post depublie");
    }

    /// <summary>Supprimer un post.</summary>
    [HttpDelete("posts/{id}")]
    public async Task<IActionResult> DeletePost(string id)
    {
        var postId = ParseGuidOrThrow(id, nameof(id), "ID du post invalide");

        var ok = await _adminService.DeletePostAsync(postId);
        if (!ok)
            return NotFoundProblem("Post non trouve");

        return SuccessMessage("Post supprime avec succes");
    }

    // ─── Events ───────────────────────────────────────────────────────────────

    /// <summary>Tous les événements (publiés et brouillons) avec pagination.</summary>
    [HttpGet("events")]
    public async Task<IActionResult> GetAllEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequestProblem("Parametres de pagination invalides");

        var events = await _adminService.GetAllEventsAsync(page, pageSize);
        return Success(events, meta: new { page, pageSize });
    }

    /// <summary>Publier un événement.</summary>
    [HttpPatch("events/{id}/publish")]
    public async Task<IActionResult> PublishEvent(string id)
    {
        var eventId = ParseGuidOrThrow(id, nameof(id), "ID de l'evenement invalide");

        var ok = await _adminService.PublishEventAsync(eventId);
        if (!ok)
            return NotFoundProblem("Evenement non trouve");

        return SuccessMessage("Evenement publie");
    }

    /// <summary>Dépublier un événement.</summary>
    [HttpPatch("events/{id}/unpublish")]
    public async Task<IActionResult> UnpublishEvent(string id)
    {
        var eventId = ParseGuidOrThrow(id, nameof(id), "ID de l'evenement invalide");

        var ok = await _adminService.UnpublishEventAsync(eventId);
        if (!ok)
            return NotFoundProblem("Evenement non trouve");

        return SuccessMessage("Evenement depublie");
    }

    /// <summary>Supprimer un événement.</summary>
    [HttpDelete("events/{id}")]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        var eventId = ParseGuidOrThrow(id, nameof(id), "ID de l'evenement invalide");

        var ok = await _adminService.DeleteEventAsync(eventId);
        if (!ok)
            return NotFoundProblem("Evenement non trouve");

        return SuccessMessage("Evenement supprime avec succes");
    }

    // ─── Comments ─────────────────────────────────────────────────────────────

    /// <summary>Supprimer un commentaire.</summary>
    [HttpDelete("comments/{id}")]
    public async Task<IActionResult> DeleteComment(string id)
    {
        var commentId = ParseGuidOrThrow(id, nameof(id), "ID du commentaire invalide");

        var ok = await _adminService.DeleteCommentAsync(commentId);
        if (!ok)
            return NotFoundProblem("Commentaire non trouve");

        return SuccessMessage("Commentaire supprime avec succes");
    }

    // ─── Projects ─────────────────────────────────────────────────────────────

    /// <summary>Tous les projets avec pagination.</summary>
    [HttpGet("projects")]
    public async Task<IActionResult> GetAllProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequestProblem("Parametres de pagination invalides");

        var projects = await _adminService.GetAllProjectsAsync(page, pageSize);
        return Success(projects, meta: new { page, pageSize });
    }

    /// <summary>Mettre en avant ou retirer un projet.</summary>
    [HttpPatch("projects/{id}/feature")]
    public async Task<IActionResult> FeatureProject(string id, [FromBody] FeatureProjectRequest request)
    {
        var projectId = ParseGuidOrThrow(id, nameof(id), "ID du projet invalide");

        var ok = await _adminService.FeatureProjectAsync(projectId, request.IsFeatured);
        if (!ok)
            return NotFoundProblem("Projet non trouve");

        return SuccessMessage(request.IsFeatured ? "Projet mis en avant" : "Projet retire de la mise en avant");
    }

    /// <summary>Supprimer un projet.</summary>
    [HttpDelete("projects/{id}")]
    public async Task<IActionResult> DeleteProject(string id)
    {
        var projectId = ParseGuidOrThrow(id, nameof(id), "ID du projet invalide");

        var ok = await _adminService.DeleteProjectAsync(projectId);
        if (!ok)
            return NotFoundProblem("Projet non trouve");

        return SuccessMessage("Projet supprime avec succes");
    }
}

