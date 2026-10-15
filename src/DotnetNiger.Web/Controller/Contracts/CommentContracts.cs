using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Commentaire publié.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="ArticleId">Article commenté, si applicable.</param>
/// <param name="CommunityEventId">Événement commenté, si applicable.</param>
/// <param name="ParentCommentId">Commentaire parent, pour une réponse.</param>
/// <param name="Body">Texte du commentaire.</param>
/// <param name="Author">Nom de l'auteur.</param>
/// <param name="CreatedAtUtc">Date de publication.</param>
public sealed record CommentResponse(
    Guid Id,
    Guid? ArticleId,
    Guid? CommunityEventId,
    Guid? ParentCommentId,
    string Body,
    string Author,
    DateTime CreatedAtUtc);

/// <summary>Demande de publication d'un commentaire.</summary>
/// <param name="Body">Texte du commentaire.</param>
/// <param name="ArticleId">Article commenté, si applicable.</param>
/// <param name="EventId">Événement commenté, si applicable.</param>
/// <param name="ParentCommentId">Commentaire parent, pour une réponse.</param>
public sealed record CreateCommentRequest(
    [Required, StringLength(4000)] string Body,
    Guid? ArticleId = null,
    Guid? EventId = null,
    Guid? ParentCommentId = null);

