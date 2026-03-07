using DotnetNiger.Community.Api.Filters;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Endpoints dédiés à l'administration de la communauté.
/// </summary>
[ApiController]
[Route("api/admin")]
[AuthorizeFilter("admin", "super-admin", "moderator")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Récupérer le tableau de bord admin (totaux et file de modération).
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _adminService.GetDashboardAsync();
        return Ok(dashboard);
    }

    /// <summary>
    /// Récupérer les commentaires en attente de modération.
    /// </summary>
    [HttpGet("moderation/comments")]
    public async Task<IActionResult> GetPendingComments([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Parametres de pagination invalides" });
        }

        var comments = await _adminService.GetPendingCommentsAsync(page, pageSize);
        return Ok(new
        {
            page,
            pageSize,
            data = comments
        });
    }

    /// <summary>
    /// Récupérer les ressources en attente de modération.
    /// </summary>
    [HttpGet("moderation/resources")]
    public async Task<IActionResult> GetPendingResources([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Parametres de pagination invalides" });
        }

        var resources = await _adminService.GetPendingResourcesAsync(page, pageSize);
        return Ok(new
        {
            page,
            pageSize,
            data = resources
        });
    }

    /// <summary>
    /// Approuver ou rejeter un commentaire.
    /// </summary>
    [HttpPatch("moderation/comments/{id}")]
    public async Task<IActionResult> ModerateComment(string id, [FromBody] ModerateRequest request)
    {
        if (!Guid.TryParse(id, out var commentId))
        {
            return BadRequest(new { message = "ID du commentaire invalide" });
        }

        var updated = await _adminService.ModerateCommentAsync(commentId, request.IsApproved);
        if (!updated)
        {
            return NotFound(new { message = "Commentaire non trouve" });
        }

        return Ok(new
        {
            message = request.IsApproved ? "Commentaire approuve" : "Commentaire rejete et supprime"
        });
    }

    /// <summary>
    /// Approuver ou rejeter une ressource.
    /// </summary>
    [HttpPatch("moderation/resources/{id}")]
    public async Task<IActionResult> ModerateResource(string id, [FromBody] ModerateRequest request)
    {
        if (!Guid.TryParse(id, out var resourceId))
        {
            return BadRequest(new { message = "ID de la ressource invalide" });
        }

        var updated = await _adminService.ModerateResourceAsync(resourceId, request.IsApproved);
        if (!updated)
        {
            return NotFound(new { message = "Ressource non trouvee" });
        }

        return Ok(new
        {
            message = request.IsApproved ? "Ressource approuvee" : "Ressource rejetee et supprimee"
        });
    }
}

public class ModerateRequest
{
    public bool IsApproved { get; set; }
}
