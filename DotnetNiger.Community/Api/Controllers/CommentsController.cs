using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les commentaires
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Récupérer tous les commentaires
    /// </summary>
    /// <returns>Liste des commentaires</returns>
    [HttpGet]
    public async Task<IActionResult> GetComments([FromQuery] int pageSize = 10)
    {
        try
        {
            var allComments = await _commentService.GetAllCommentsAsync();
            return Ok(new { data = allComments.Take(pageSize) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des commentaires", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupérer un commentaire par ID
    /// </summary>
    /// <param name="id">ID du commentaire</param>
    /// <returns>Détails du commentaire</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCommentById(string id)
    {
        if (!Guid.TryParse(id, out var commentId))
            return BadRequest(new { message = "ID du commentaire invalide" });

        try
        {
            var comment = await _commentService.GetCommentByIdAsync(commentId);
            if (comment == null)
                return NotFound(new { message = "Commentaire non trouvé" });

            return Ok(comment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération du commentaire", error = ex.Message });
        }
    }

    /// <summary>
    /// Créer un nouveau commentaire
    /// </summary>
    /// <param name="request">Données du commentaire</param>
    /// <returns>Commentaire créé</returns>
    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Content))
            return BadRequest(new { message = "Contenu requis" });

        if (!Guid.TryParse(request.PostId, out var postId))
            return BadRequest(new { message = "ID du post invalide" });

        if (!this.TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(new { message = "Utilisateur non authentifie", details = "Claim user ou header X-User-Id requis" });

        try
        {
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = currentUserId,
                Content = request.Content,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentService.CreateCommentAsync(comment);
            return CreatedAtAction(nameof(GetCommentById), new { id = createdComment.Id }, createdComment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la création du commentaire", error = ex.Message });
        }
    }

    /// <summary>
    /// Mettre à jour un commentaire
    /// </summary>
    /// <param name="id">ID du commentaire</param>
    /// <param name="request">Contenu à mettre à jour</param>
    /// <returns>Commentaire mis à jour</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComment(string id, [FromBody] UpdateCommentRequest request)
    {
        if (!Guid.TryParse(id, out var commentId))
            return BadRequest(new { message = "ID du commentaire invalide" });

        try
        {
            var comment = await _commentService.GetCommentByIdAsync(commentId);
            if (comment == null)
                return NotFound(new { message = "Commentaire non trouvé" });

            if (!string.IsNullOrEmpty(request.Content))
                comment.Content = request.Content;

            comment.UpdatedAt = DateTime.UtcNow;
            var updatedComment = await _commentService.UpdateCommentAsync(comment);
            return Ok(updatedComment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la mise à jour du commentaire", error = ex.Message });
        }
    }

    /// <summary>
    /// Supprimer un commentaire
    /// </summary>
    /// <param name="id">ID du commentaire</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(string id)
    {
        if (!Guid.TryParse(id, out var commentId))
            return BadRequest(new { message = "ID du commentaire invalide" });

        try
        {
            var deleted = await _commentService.DeleteCommentAsync(commentId);
            if (!deleted)
                return NotFound(new { message = "Commentaire non trouvé" });

            return Ok(new { message = "Commentaire supprimé avec succès" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la suppression du commentaire", error = ex.Message });
        }
    }
}

/// <summary>
/// DTO pour créer un commentaire
/// </summary>
public class CreateCommentRequest
{
    /// <summary>Contenu du commentaire</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>ID du post concerné</summary>
    public string PostId { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour mettre à jour un commentaire
/// </summary>
public class UpdateCommentRequest
{
    /// <summary>Contenu du commentaire</summary>
    public string? Content { get; set; }
}
