using Asp.Versioning;
using DotnetNiger.Community.Application;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Common.Constants;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des articles de blog.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PostsController(
    IPostQueryService postQuery,
    IPostCommandService postCommand,
    IPostModerationService postModeration,
    ICertificateService certificateService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? published, [FromQuery] string? category, [FromQuery] string? tag, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 6, [FromQuery] Guid? after = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, ValidationConstants.MaxPageSize);
        return Ok(new { Success = true, Data = await postQuery.GetAllAsync(published, category, tag, query, page, pageSize, after) });
    }

    [HttpGet("{id:guid}", Order = 1)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var post = await postQuery.GetByIdAsync(id);
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    [HttpGet("{slug:regex(^[[a-z0-9]]+(?:-[[a-z0-9]]+)*$)}", Order = 2)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var post = await postQuery.GetBySlugAsync(slug);
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<OGMetadata>> GetOGBySlug(string slug)
    {
        var post = await postQuery.GetBySlugAsync(slug);
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new ApiSuccessResponse<OGMetadata>
        {
            Data = new OGMetadata { Title = post.Title, Description = post.Excerpt, ImageUrl = post.CoverImageUrl, UpdatedAt = post.UpdatedAt }
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
    {
        var userId = GetUserId();
        var (canCreate, forceUnpublished, error) = await certificateService.CanCreateContentAsync(userId, User);
        if (!canCreate)
            return error != null ? BadRequest(new { Success = false, Message = error }) : Forbid();
        if (forceUnpublished) request.IsPublished = false;
        var post = await postCommand.CreateAsync(request, userId, GetUserName());
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, new { Success = true, Data = post });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest request)
    {
        var post = await postCommand.UpdateAsync(id, request, GetUserId(), IsAdmin());
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    [HttpPatch("{id:guid}/publish")]
    [Authorize]
    public async Task<IActionResult> Publish(Guid id)
    {
        var post = await postModeration.PublishAsync(id, GetUserId(), IsAdmin());
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    [HttpPatch("{id:guid}/unpublish")]
    [Authorize]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        var post = await postModeration.UnpublishAsync(id, GetUserId(), IsAdmin());
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    [HttpPost("{id:guid}/views")]
    public async Task<IActionResult> IncrementViewCount(Guid id)
    {
        var post = await postCommand.IncrementViewCountAsync(id);
        if (post is null) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Data = post });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await postCommand.DeleteAsync(id, GetUserId(), IsAdmin());
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Post.NotFound });
        return Ok(new { Success = true, Message = Messages.Post.Deleted });
    }
}
