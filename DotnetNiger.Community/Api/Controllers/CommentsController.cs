using Asp.Versioning;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des commentaires sur les articles et événements.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CommentsController(ICommentService commentService) : BaseController
{
    /// <summary>Récupère tous les commentaires d'un article.</summary>
    /// <param name="postId">Identifiant de l'article.</param>
    [HttpGet("post/{postId:guid}")]
    public async Task<IActionResult> GetByPostId(Guid postId)
    {
        var comments = await commentService.GetByPostIdAsync(postId);
        return Ok(new { Success = true, Data = comments });
    }

    /// <summary>Récupère tous les commentaires d'un événement.</summary>
    /// <param name="eventId">Identifiant de l'événement.</param>
    [HttpGet("event/{eventId:guid}")]
    public async Task<IActionResult> GetByEventId(Guid eventId)
    {
        var comments = await commentService.GetByEventIdAsync(eventId);
        return Ok(new { Success = true, Data = comments });
    }

    /// <summary>Recherche un commentaire par son identifiant.</summary>
    /// <param name="id">Identifiant du commentaire.</param>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var comment = await commentService.GetByIdAsync(id);
        if (comment is null) return NotFound(new { Success = false, Message = Messages.Comment.NotFound });
        return Ok(new { Success = true, Data = comment });
    }

    /// <summary>Ajoute un commentaire sur un article ou un événement.</summary>
    /// <param name="request">Contenu et cible du commentaire.</param>
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

    /// <summary>Modifie un commentaire (seul l'auteur peut modifier).</summary>
    /// <param name="id">Identifiant du commentaire.</param>
    /// <param name="request">Nouveau contenu.</param>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentRequest request)
    {
        var userId = GetUserId();
        var comment = await commentService.UpdateAsync(id, request, userId);
        if (comment is null) return NotFound(new { Success = false, Message = Messages.Comment.NotFound });
        return Ok(new { Success = true, Data = comment });
    }

    /// <summary>Supprime un commentaire (auteur ou admin).</summary>
    /// <param name="id">Identifiant du commentaire.</param>
    /// <param name="deleteAllReplies">Supprime aussi les réponses si vrai.</param>
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
