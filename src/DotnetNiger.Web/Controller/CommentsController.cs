using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Commentaires d'articles et d'événements.
/// </summary>
[Route("api/comments")]
[Tags("Commentaires")]
public sealed class CommentsController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ILogger<CommentsController> logger) : ApiControllerBase
{
    /// <summary>Liste les commentaires publiés, filtrables par cible.</summary>
    /// <param name="articleId">Filtre par article.</param>
    /// <param name="eventId">Filtre par événement.</param>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de commentaires.</returns>
    /// <response code="200">Page de commentaires.</response>
    /// <response code="400">Les deux filtres ont été fournis.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PageResponse<CommentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PageResponse<CommentResponse>>> Lister(
        [FromQuery] Guid? articleId = null,
        [FromQuery] Guid? eventId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Un commentaire porte soit sur un article, soit sur un événement : accepter les deux
        // filtres à la fois renverrait une page vide sans erreur, ce qui masque une erreur
        // d'appel plutôt que de la signaler.
        if (articleId is { } cibleArticle && eventId is { } cibleEvenement)
        {
            throw new DomainException(
                "invalid_filter",
                "Un commentaire se rattache à un article ou à un événement, pas aux deux.");
        }

        var requete = unitOfWork.ReadRepository<Comment>().Query
            .Where(comment => comment.Status == CommentStatus.Published);

        if (articleId is { } article)
        {
            requete = requete.Where(comment => comment.ArticleId == article);
        }

        if (eventId is { } evenement)
        {
            requete = requete.Where(comment => comment.CommunityEventId == evenement);
        }

        requete = requete.OrderBy(comment => comment.CreatedAtUtc);

        return await PageOfAsync(
            requete,
            new PageRequest(page, pageSize),
            comment => ApiProjections.ToResponse(comment, string.Empty),
            cancellationToken);
    }

    /// <summary>Dépose un commentaire sur un article ou un événement.</summary>
    /// <param name="request">Contenu du commentaire et sa cible.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'identifiant du commentaire et son état.</returns>
    /// <remarks>
    /// Le premier commentaire d'une cible et les réponses sont mis en attente de relecture :
    /// la discussion est modérée a priori, le domaine porte l'état et l'API le demande.
    /// </remarks>
    /// <response code="201">Commentaire déposé.</response>
    /// <response code="400">Cible absente ou commentaire parent incohérent.</response>
    /// <response code="404">Article, événement ou parent introuvable.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deposer(
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var auteur = RequireUser(currentUser);

        if (request.ArticleId is null && request.EventId is null)
        {
            throw new DomainException("invalid_target", "Indiquez l'article ou l'événement commenté.");
        }

        if (request.ArticleId is { } articleId
            && !await unitOfWork.ReadRepository<Article>().Query
                .AnyAsync(article => article.Id == articleId, cancellationToken))
        {
            throw new DomainException("not_found", "Cet article n'existe pas.");
        }

        if (request.EventId is { } eventId
            && !await unitOfWork.ReadRepository<CommunityEvent>().Query
                .AnyAsync(communityEvent => communityEvent.Id == eventId, cancellationToken))
        {
            throw new DomainException("not_found", "Cet événement n'existe pas.");
        }

        if (request.ParentCommentId is { } parentCommentId)
        {
            var parent = await unitOfWork.ReadRepository<Comment>().Query
                .FirstOrDefaultAsync(comment => comment.Id == parentCommentId, cancellationToken)
                ?? throw new DomainException("not_found", "Ce commentaire parent n'existe pas.");

            // Répondre hors sujet, donc à un commentaire d'une autre cible, produirait une
            // conversation qui ne s'affiche jamais.
            if (parent.ArticleId != request.ArticleId || parent.CommunityEventId != request.EventId)
            {
                throw new DomainException(
                    "invalid_parent",
                    "Le commentaire parent ne porte pas sur la même cible.");
            }
        }

        var commentaire = request.ArticleId is { } idArticle
            ? Comment.ForArticle(idArticle, auteur, request.Body, request.ParentCommentId)
            : Comment.ForCommunityEvent(
                request.EventId!.Value,
                auteur,
                request.Body,
                request.ParentCommentId);

        if (await EstPremierCommentAsync(request, cancellationToken))
        {
            commentaire.MarkPendingReview();
        }

        unitOfWork.Repository<Comment>().Add(commentaire);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApiLog.CommentaireDepose(logger, commentaire.Id, auteur, commentaire.Status);

        return Created(
            $"/api/comments/{commentaire.Id}",
            new { commentaire.Id, commentaire.Status, CreatedAtUtc = commentaire.CreatedAtUtc });
    }

    /// <summary>
    /// Indique si la cible du commentaire ne porte encore aucun commentaire.
    /// </summary>
    /// <param name="request">Demande en cours de traitement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns><see langword="true"/> si c'est le premier commentaire de la cible.</returns>
    /// <remarks>
    /// Les valeurs sont extraites avant l'arbre d'expression : le motif de déconstruction
    /// n'y est pas accepté, contrairement à une comparaison simple.
    /// </remarks>
    private Task<bool> EstPremierCommentAsync(
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var articleId = request.ArticleId;
        var eventId = request.EventId;

        return unitOfWork.ReadRepository<Comment>().Query
            .AnyAsync(
                comment => (articleId != null && comment.ArticleId == articleId)
                    || (eventId != null && comment.CommunityEventId == eventId),
                cancellationToken);
    }
}
