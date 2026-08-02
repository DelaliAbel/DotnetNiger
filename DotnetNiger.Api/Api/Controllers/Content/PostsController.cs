using DotnetNiger.Api.Application.Interfaces;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Content;

/// <summary>Contrôleur de gestion des articles (posts).</summary>
[ApiController]
[Route("api/posts")]
public class PostsController(
    IPostQueryService postQuery,
    IPostCommandService postCommand,
    IPostModerationService postModeration) : BaseController
{
    /// <summary>Récupère la liste paginée des articles avec filtres optionnels.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? published, [FromQuery] string? category, [FromQuery] string? tag, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 6, [FromQuery] Guid? after = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        published = published == "true" ? published : "true";
        return Success(await postQuery.GetAllAsync(published, category, tag, query, page, pageSize, after));
    }

    /// <summary>Récupère les articles de l'utilisateur connecté.</summary>
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var userId = GetUserId();
        return Success(await postQuery.GetAllAsync(null, null, null, null, page, pageSize, null, userId));
    }

    /// <summary>Récupère tous les articles (admin - tous statuts).</summary>
    [HttpGet("admin")]
    [Authorize(Policy = "admin.dashboard.view")]
    public async Task<IActionResult> GetAdminAll([FromQuery] string? published, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        return Success(await postQuery.GetAllAsync(published, null, null, query, page, pageSize, null));
    }

    /// <summary>Récupère un article par son identifiant.</summary>
    [HttpGet("{id:guid}", Order = 1)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var post = await postQuery.GetByIdAsync(id);
        if (post is null) return NotFound(Messages.Post.NotFound);
        if (post.Status != nameof(PostStatus.Published) && !CanViewUnpublished(post))
            return NotFound(Messages.Post.NotFound);
        return Success(post);
    }

    /// <summary>Récupère un article par son slug.</summary>
    [HttpGet("{slug:regex(^[[a-z0-9]]+(?:-[[a-z0-9]]+)*$)}", Order = 2)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var post = await postQuery.GetBySlugAsync(slug);
        if (post is null) return NotFound(Messages.Post.NotFound);
        if (post.Status != nameof(PostStatus.Published) && !CanViewUnpublished(post))
            return NotFound(Messages.Post.NotFound);
        return Success(post);
    }

    /// <summary>Récupère les métadonnées Open Graph d'un article par son slug.</summary>
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetOGBySlug(string slug)
    {
        var post = await postQuery.GetBySlugAsync(slug);
        if (post is null) return NotFound(Messages.Post.NotFound);
        if (post.Status != nameof(PostStatus.Published) && !CanViewUnpublished(post))
            return NotFound(Messages.Post.NotFound);
        return Success(new OGMetadata { Title = post.Title, Description = post.Excerpt, ImageUrl = post.CoverImageUrl, UpdatedAt = post.UpdatedAt });
    }

    private bool CanViewUnpublished(PostResponse post)
    {
        if (!User.Identity?.IsAuthenticated == true) return false;
        var userId = GetUserId();
        return IsAdmin() || IsCollaborator() || post.AuthorId == userId;
    }

    /// <summary>Crée un nouvel article.</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
    {
        var userId = GetUserId();
        try
        {
            var post = await postCommand.CreateAsync(request, userId, GetUserName(), IsAdmin(), IsCollaborator());
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, new { success = true, data = post, message = (string?)null });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Met à jour un article existant.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest request)
    {
        try
        {
            var post = await postCommand.UpdateAsync(id, request, GetUserId(), IsAdmin());
            if (post is null) return NotFound(Messages.Post.NotFound);
            return Success(post);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, 403);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Publie un article (le rend visible).</summary>
    [HttpPatch("{id:guid}/publish")]
    [Authorize]
    public async Task<IActionResult> Publish(Guid id)
    {
        try
        {
            var post = await postModeration.PublishAsync(id, GetUserId(), IsAdmin());
            if (post is null) return NotFound(Messages.Post.NotFound);
            return Success(post);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, 403);
        }
    }

    /// <summary>Dépublie un article (le rend invisible).</summary>
    [HttpPatch("{id:guid}/unpublish")]
    [Authorize]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        try
        {
            var post = await postModeration.UnpublishAsync(id, GetUserId(), IsAdmin());
            if (post is null) return NotFound(Messages.Post.NotFound);
            return Success(post);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, 403);
        }
    }

    /// <summary>Incrémente le compteur de vues d'un article.</summary>
    [HttpPost("{id:guid}/views")]
    public async Task<IActionResult> IncrementViewCount(Guid id)
    {
        var post = await postCommand.IncrementViewCountAsync(id);
        if (post is null) return NotFound(Messages.Post.NotFound);
        return Success(post);
    }

    /// <summary>Supprime un article.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await postCommand.DeleteAsync(id, GetUserId(), IsAdmin());
            if (!deleted) return NotFound(Messages.Post.NotFound);
            return Success<object?>(null, Messages.Post.Deleted);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, 403);
        }
    }
}
