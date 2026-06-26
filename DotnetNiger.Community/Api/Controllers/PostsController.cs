using Asp.Versioning;
using DotnetNiger.Community.Application;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des articles de blog de la communauté.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PostsController(IPostService postService, IProfileService profileService) : BaseController
{
    /// <summary>Recherche et filtre les articles avec pagination.</summary>
    /// <param name="published">Filtre publication ("true"/"false").</param>
    /// <param name="category">Filtre par catégorie.</param>
    /// <param name="tag">Filtre par tag.</param>
    /// <param name="query">Recherche textuelle.</param>
    /// <param name="page">Page demandée.</param>
    /// <param name="pageSize">Taille de la page.</param>
    /// <param name="after">Curseur pour la pagination.</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? published, [FromQuery] string? category, [FromQuery] string? tag, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 6, [FromQuery] Guid? after = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        var result = await postService.GetAllAsync(published, category, tag, query, page, pageSize, after);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>Recherche un article par son identifiant.</summary>
    /// <param name="id">Identifiant de l'article.</param>
    [HttpGet("{id:guid}", Order = 1)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var post = await postService.GetByIdAsync(id);
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    /// <summary>Recherche un article par son slug.</summary>
    /// <param name="slug">Slug de l'article.</param>
    [HttpGet("{slug:regex(^[[a-z0-9]]+(?:-[[a-z0-9]]+)*$)}", Order = 2)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var post = await postService.GetBySlugAsync(slug);
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    /// <summary>Récupère les métadonnées Open Graph d'un article pour le partage social.</summary>
    /// <param name="slug">Slug de l'article.</param>
    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<OGMetadata>> GetOGBySlug(string slug)
    {
        var post = await postService.GetBySlugAsync(slug);
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });

        return Ok(new ApiSuccessResponse<OGMetadata>
        {
            Data = new OGMetadata
            {
                Title = post.Title,
                Description = post.Excerpt,
                ImageUrl = post.CoverImageUrl,
                UpdatedAt = post.UpdatedAt
            }
        });
    }

    /// <summary>Crée un nouvel article. Les collaborateurs doivent avoir un certificat validé.</summary>
    /// <param name="request">Contenu et métadonnées de l'article.</param>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
    {
        var userId = GetUserId();

        if (!IsAdmin())
        {
            if (!IsCollaborator())
                return Forbid();

            var hasCert = await profileService.HasApprovedCertificateAsync(userId);
            if (!hasCert)
                return BadRequest(new { Success = false, Message = Messages.Certificate.NeedValidCertificate });

            request.IsPublished = false;
        }

        var userName = GetUserName();
        var post = await postService.CreateAsync(request, userId, userName);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, new { Success = true, Data = post });
    }

    /// <summary>Modifie un article existant (auteur ou admin).</summary>
    /// <param name="id">Identifiant de l'article.</param>
    /// <param name="request">Nouvelles informations.</param>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest request)
    {
        var userId = GetUserId();
        var post = await postService.UpdateAsync(id, request, userId, IsAdmin());
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    /// <summary>Publie un article (auteur ou admin).</summary>
    /// <param name="id">Identifiant de l'article.</param>
    [HttpPatch("{id:guid}/publish")]
    [Authorize]
    public async Task<IActionResult> Publish(Guid id)
    {
        var userId = GetUserId();
        var post = await postService.PublishAsync(id, userId, IsAdmin());
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    /// <summary>Dépublie un article (auteur ou admin).</summary>
    /// <param name="id">Identifiant de l'article.</param>
    [HttpPatch("{id:guid}/unpublish")]
    [Authorize]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        var userId = GetUserId();
        var post = await postService.UnpublishAsync(id, userId, IsAdmin());
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    /// <summary>Incémente le compteur de vues d'un article.</summary>
    /// <param name="id">Identifiant de l'article.</param>
    [HttpPost("{id:guid}/views")]
    public async Task<IActionResult> IncrementViewCount(Guid id)
    {
        var post = await postService.IncrementViewCountAsync(id);
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    /// <summary>Supprime un article (auteur ou admin).</summary>
    /// <param name="id">Identifiant de l'article.</param>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = await postService.DeleteAsync(id, userId, IsAdmin());
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Message = Messages.Post.Deleted });
    }
}
