using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Api.Services;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Mappers;
using DotnetNiger.Community.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les commentaires
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/comments")]
public class CommentsController : ApiControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommunityRequestMapper _requestMapper;

    public CommentsController(
        ICommentService commentService,
        ICurrentUserService currentUserService,
        ICommunityRequestMapper requestMapper)
    {
        _commentService = commentService;
        _currentUserService = currentUserService;
        _requestMapper = requestMapper;
    }

    /// <summary>
    /// Récupérer tous les commentaires
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Nombre de commentaires par page (par défaut 20)</param>
    /// <returns>Liste paginée des commentaires</returns>
    [HttpGet]
    public async Task<IActionResult> GetComments([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequestProblem("Parametres de pagination invalides");

        var comments = await _commentService.GetAllCommentsAsync(page, pageSize);
        return Success(comments, meta: new { page, pageSize });
    }

    /// <summary>
    /// Récupérer un commentaire par ID
    /// </summary>
    /// <param name="id">ID du commentaire</param>
    /// <returns>Détails du commentaire</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCommentById(string id)
    {
        var commentId = ParseGuidOrThrow(id, nameof(id), "ID du commentaire invalide");

        var comment = await _commentService.GetCommentByIdAsync(commentId);
        if (comment == null)
            return NotFoundProblem("Commentaire non trouve");

        return Success(comment);
    }

    /// <summary>
    /// Créer un nouveau commentaire
    /// </summary>
    /// <param name="request">Données du commentaire</param>
    /// <returns>Commentaire créé</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Content))
            return BadRequestProblem("Contenu requis");

        var postId = ParseGuidOrThrow(request.PostId, nameof(request.PostId), "ID du post invalide");
        var currentUserId = _currentUserService.GetRequiredUserId();
        var comment = _requestMapper.MapToComment(request, postId, currentUserId);

        var createdComment = await _commentService.CreateCommentAsync(comment);
        return CreatedSuccess(nameof(GetCommentById), new { id = createdComment.Id }, createdComment, "Commentaire cree avec succes");
    }

    /// <summary>
    /// Mettre à jour un commentaire
    /// </summary>
    /// <param name="id">ID du commentaire</param>
    /// <param name="request">Contenu à mettre à jour</param>
    /// <returns>Commentaire mis à jour</returns>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(string id, [FromBody] UpdateCommentRequest request)
    {
        var commentId = ParseGuidOrThrow(id, nameof(id), "ID du commentaire invalide");

        var comment = await _commentService.GetCommentByIdAsync(commentId);
        if (comment == null)
            return NotFoundProblem("Commentaire non trouve");

        var currentUserId = _currentUserService.GetRequiredUserId();
        var canModerate = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        if (comment.UserId != currentUserId && !canModerate)
            return Forbid();

        _requestMapper.ApplyCommentUpdates(comment, request);
        var updatedComment = await _commentService.UpdateCommentAsync(comment);
        return Success(updatedComment, "Commentaire mis a jour avec succes");
    }

    /// <summary>
    /// Supprimer un commentaire
    /// </summary>
    /// <param name="id">ID du commentaire</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(string id)
    {
        var commentId = ParseGuidOrThrow(id, nameof(id), "ID du commentaire invalide");

        var comment = await _commentService.GetCommentByIdAsync(commentId);
        if (comment == null)
            return NotFoundProblem("Commentaire non trouve");

        var currentUserId = _currentUserService.GetRequiredUserId();
        var canModerate = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        if (comment.UserId != currentUserId && !canModerate)
            return Forbid();

        var deleted = await _commentService.DeleteCommentAsync(commentId);
        if (!deleted)
            return NotFoundProblem("Commentaire non trouve");

        return SuccessMessage("Commentaire supprime avec succes");
    }
}

