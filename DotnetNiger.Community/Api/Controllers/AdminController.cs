using Asp.Versioning;
using DotnetNiger.Community.Api.Filters;
using DotnetNiger.Community.Application.Services;
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
public class AdminController : ControllerBase
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
        return Ok(dashboard);
    }

    // ─── Resources ────────────────────────────────────────────────────────────

    /// <summary>Ressources en attente de modération.</summary>
    [HttpGet("resources")]
    public async Task<IActionResult> GetPendingResources([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Parametres de pagination invalides" });

        var resources = await _adminService.GetPendingResourcesAsync(page, pageSize);
        return Ok(new { page, pageSize, data = resources });
    }

    /// <summary>Approuver ou rejeter une ressource.</summary>
    [HttpPatch("resources/{id}")]
    public async Task<IActionResult> ModerateResource(string id, [FromBody] ModerateRequest request)
    {
        if (!Guid.TryParse(id, out var resourceId))
            return BadRequest(new { message = "ID de la ressource invalide" });

        var updated = await _adminService.ModerateResourceAsync(resourceId, request.IsApproved);
        if (!updated)
            return NotFound(new { message = "Ressource non trouvee" });

        return Ok(new { message = request.IsApproved ? "Ressource approuvee" : "Ressource rejetee et supprimee" });
    }

    // ─── Posts ────────────────────────────────────────────────────────────────

    /// <summary>Tous les posts (publiés et brouillons) avec pagination.</summary>
    [HttpGet("posts")]
    public async Task<IActionResult> GetAllPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Parametres de pagination invalides" });

        var posts = await _adminService.GetAllPostsAsync(page, pageSize);
        return Ok(new { page, pageSize, data = posts });
    }

    /// <summary>Publier un post.</summary>
    [HttpPatch("posts/{id}/publish")]
    public async Task<IActionResult> PublishPost(string id)
    {
        if (!Guid.TryParse(id, out var postId))
            return BadRequest(new { message = "ID du post invalide" });

        var ok = await _adminService.PublishPostAsync(postId);
        if (!ok)
            return NotFound(new { message = "Post non trouve" });

        return Ok(new { message = "Post publie" });
    }

    /// <summary>Dépublier un post.</summary>
    [HttpPatch("posts/{id}/unpublish")]
    public async Task<IActionResult> UnpublishPost(string id)
    {
        if (!Guid.TryParse(id, out var postId))
            return BadRequest(new { message = "ID du post invalide" });

        var ok = await _adminService.UnpublishPostAsync(postId);
        if (!ok)
            return NotFound(new { message = "Post non trouve" });

        return Ok(new { message = "Post depublie" });
    }

    /// <summary>Supprimer un post.</summary>
    [HttpDelete("posts/{id}")]
    public async Task<IActionResult> DeletePost(string id)
    {
        if (!Guid.TryParse(id, out var postId))
            return BadRequest(new { message = "ID du post invalide" });

        var ok = await _adminService.DeletePostAsync(postId);
        if (!ok)
            return NotFound(new { message = "Post non trouve" });

        return NoContent();
    }

    // ─── Events ───────────────────────────────────────────────────────────────

    /// <summary>Tous les événements (publiés et brouillons) avec pagination.</summary>
    [HttpGet("events")]
    public async Task<IActionResult> GetAllEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Parametres de pagination invalides" });

        var events = await _adminService.GetAllEventsAsync(page, pageSize);
        return Ok(new { page, pageSize, data = events });
    }

    /// <summary>Publier un événement.</summary>
    [HttpPatch("events/{id}/publish")]
    public async Task<IActionResult> PublishEvent(string id)
    {
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest(new { message = "ID de l'evenement invalide" });

        var ok = await _adminService.PublishEventAsync(eventId);
        if (!ok)
            return NotFound(new { message = "Evenement non trouve" });

        return Ok(new { message = "Evenement publie" });
    }

    /// <summary>Dépublier un événement.</summary>
    [HttpPatch("events/{id}/unpublish")]
    public async Task<IActionResult> UnpublishEvent(string id)
    {
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest(new { message = "ID de l'evenement invalide" });

        var ok = await _adminService.UnpublishEventAsync(eventId);
        if (!ok)
            return NotFound(new { message = "Evenement non trouve" });

        return Ok(new { message = "Evenement depublie" });
    }

    /// <summary>Supprimer un événement.</summary>
    [HttpDelete("events/{id}")]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest(new { message = "ID de l'evenement invalide" });

        var ok = await _adminService.DeleteEventAsync(eventId);
        if (!ok)
            return NotFound(new { message = "Evenement non trouve" });

        return NoContent();
    }

    // ─── Comments ─────────────────────────────────────────────────────────────

    /// <summary>Supprimer un commentaire.</summary>
    [HttpDelete("comments/{id}")]
    public async Task<IActionResult> DeleteComment(string id)
    {
        if (!Guid.TryParse(id, out var commentId))
            return BadRequest(new { message = "ID du commentaire invalide" });

        var ok = await _adminService.DeleteCommentAsync(commentId);
        if (!ok)
            return NotFound(new { message = "Commentaire non trouve" });

        return NoContent();
    }

    // ─── Projects ─────────────────────────────────────────────────────────────

    /// <summary>Tous les projets avec pagination.</summary>
    [HttpGet("projects")]
    public async Task<IActionResult> GetAllProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Parametres de pagination invalides" });

        var projects = await _adminService.GetAllProjectsAsync(page, pageSize);
        return Ok(new { page, pageSize, data = projects });
    }

    /// <summary>Mettre en avant ou retirer un projet.</summary>
    [HttpPatch("projects/{id}/feature")]
    public async Task<IActionResult> FeatureProject(string id, [FromBody] FeatureRequest request)
    {
        if (!Guid.TryParse(id, out var projectId))
            return BadRequest(new { message = "ID du projet invalide" });

        var ok = await _adminService.FeatureProjectAsync(projectId, request.IsFeatured);
        if (!ok)
            return NotFound(new { message = "Projet non trouve" });

        return Ok(new { message = request.IsFeatured ? "Projet mis en avant" : "Projet retire de la mise en avant" });
    }

    /// <summary>Supprimer un projet.</summary>
    [HttpDelete("projects/{id}")]
    public async Task<IActionResult> DeleteProject(string id)
    {
        if (!Guid.TryParse(id, out var projectId))
            return BadRequest(new { message = "ID du projet invalide" });

        var ok = await _adminService.DeleteProjectAsync(projectId);
        if (!ok)
            return NotFound(new { message = "Projet non trouve" });

        return NoContent();
    }
}

public class ModerateRequest
{
    public bool IsApproved { get; set; }
}

public class FeatureRequest
{
    public bool IsFeatured { get; set; }
}

