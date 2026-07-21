using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Content;

[ApiController]
[Route("api/comments")]
public class CommentsController(ICommentService commentService) : BaseController
{
    [HttpGet("post/{postId:guid}")]
    public async Task<IActionResult> GetByPostId(Guid postId)
    {
        var comments = await commentService.GetByPostIdAsync(postId);
        return Ok(new { Success = true, Data = comments });
    }

    [HttpGet("event/{eventId:guid}")]
    public async Task<IActionResult> GetByEventId(Guid eventId)
    {
        var comments = await commentService.GetByEventIdAsync(eventId);
        return Ok(new { Success = true, Data = comments });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var comment = await commentService.GetByIdAsync(id);
        if (comment is null) return NotFound(new { Success = false, Message = Messages.Comment.NotFound });
        return Ok(new { Success = true, Data = comment });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateCommentRequest request)
    {
        var userId = GetUserId();
        var userName = GetUserName();
        var avatar = GetUserAvatar();
        var comment = await commentService.CreateAsync(request, userId, userName, avatar);
        return Ok(new { Success = true, Data = comment });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentRequest request)
    {
        var userId = GetUserId();
        var comment = await commentService.UpdateAsync(id, request, userId);
        if (comment is null) return NotFound(new { Success = false, Message = Messages.Comment.NotFound });
        return Ok(new { Success = true, Data = comment });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] bool deleteAllReplies = false)
    {
        var userId = GetUserId();
        var deleted = await commentService.DeleteAsync(id, userId, deleteAllReplies);
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Comment.NotFound });
        return Ok(new { Success = true, Message = Messages.Comment.Deleted });
    }
}
