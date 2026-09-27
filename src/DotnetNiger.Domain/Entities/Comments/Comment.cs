using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Entities.Comments;

/// <summary>
/// Commentaire posté par un membre connecté sur un article de blog ou sur un événement.
/// </summary>
/// <remarks>
/// Les commentaires ne sont volontairement pas autorisés sur les ressources ni sur les
/// projets : seules deux cibles sont acceptées, et une contrainte de base de données
/// impose qu'exactement l'une des deux soit renseignée.
/// <para>
///Conformément à la décision de modération, un commentaire est publié immédiatement.
/// Le signalement déclenche un réexamen a posteriori par un administrateur.
/// </para>
/// </remarks>
public sealed class Comment : SoftDeletableEntity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private Comment()
    {
    }

    private Comment(string body, Guid authorId, Guid? parentCommentId)
    {
        Body = body;
        AuthorId = authorId;
        ParentCommentId = parentCommentId;
        Status = CommentStatus.Published;
    }

    /// <summary>
    /// Publie un commentaire sur un article de blog.
    /// </summary>
    /// <param name="articleId">Identifiant de l'article.</param>
    /// <param name="authorId">Identifiant du membre auteur.</param>
    /// <param name="body">Texte du commentaire.</param>
    /// <param name="parentCommentId">Commentaire parent, pour une réponse.</param>
    /// <returns>Nouveau commentaire.</returns>
    public static Comment ForArticle(Guid articleId, Guid authorId, string body, Guid? parentCommentId = null)
    {
        DomainGuard.AgainstEmpty(articleId, nameof(articleId));

        var comment = Create(body, authorId, parentCommentId);
        comment.ArticleId = articleId;
        return comment;
    }

    /// <summary>
    /// Publie un commentaire sur un événement.
    /// </summary>
    /// <param name="communityEventId">Identifiant de l'événement.</param>
    /// <param name="authorId">Identifiant du membre auteur.</param>
    /// <param name="body">Texte du commentaire.</param>
    /// <param name="parentCommentId">Commentaire parent, pour une réponse.</param>
    /// <returns>Nouveau commentaire.</returns>
    public static Comment ForCommunityEvent(
        Guid communityEventId,
        Guid authorId,
        string body,
        Guid? parentCommentId = null)
    {
        DomainGuard.AgainstEmpty(communityEventId, nameof(communityEventId));

        var comment = Create(body, authorId, parentCommentId);
        comment.CommunityEventId = communityEventId;
        return comment;
    }

    /// <summary>
    /// Répond à un commentaire existant. La réponse hérite de la cible du parent.
    /// </summary>
    /// <param name="parent">Commentaire parent.</param>
    /// <param name="authorId">Identifiant du membre auteur de la réponse.</param>
    /// <param name="body">Texte de la réponse.</param>
    /// <returns>Nouveau commentaire rattaché à la même cible.</returns>
    /// <exception cref="DomainException">Le commentaire parent n'a aucune cible valide.</exception>
    public static Comment ReplyTo(Comment parent, Guid authorId, string body)
    {
        ArgumentNullException.ThrowIfNull(parent);

        if (parent.ArticleId is not null)
        {
            return ForArticle(parent.ArticleId.Value, authorId, body, parent.Id);
        }

        if (parent.CommunityEventId is not null)
        {
            return ForCommunityEvent(parent.CommunityEventId.Value, authorId, body, parent.Id);
        }

        throw new DomainException(
            "comment_invalid_parent",
            "Le commentaire parent n'est rattaché ni à un article ni à un événement.");
    }

    /// <summary>
    /// Obtient le texte du commentaire.
    /// </summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient l'état de modération.
    /// </summary>
    public CommentStatus Status { get; private set; }

    /// <summary>
    /// Obtient l'identifiant du membre auteur.
    /// </summary>
    public Guid AuthorId { get; private set; }

    /// <summary>
    /// Obtient l'identifiant de l'article commenté, si la cible est un article.
    /// </summary>
    public Guid? ArticleId { get; private set; }

    /// <summary>
    /// Obtient l'identifiant de l'événement commenté, si la cible est un événement.
    /// </summary>
    public Guid? CommunityEventId { get; private set; }

    /// <summary>
    /// Obtient l'identifiant du commentaire parent, s'il s'agit d'une réponse.
    /// </summary>
    public Guid? ParentCommentId { get; private set; }

    /// <summary>
    /// Obtient l'article commenté, si la cible est un article.
    /// </summary>
    public Article? Article { get; private set; }

    /// <summary>
    /// Obtient l'événement commenté, si la cible est un événement.
    /// </summary>
    public CommunityEvent? CommunityEvent { get; private set; }

    /// <summary>
    /// Obtient le commentaire parent, s'il s'agit d'une réponse.
    /// </summary>
    public Comment? ParentComment { get; private set; }

    /// <summary>
    /// Place le commentaire en attente de réexamen, par exemple après un signalement.
    /// </summary>
    public void MarkPendingReview() => Status = CommentStatus.PendingReview;

    /// <summary>
    /// Masque le commentaire sans le supprimer. L'historique reste consultable par l'administration.
    /// </summary>
    public void Hide() => Status = CommentStatus.Hidden;

    /// <summary>
    /// Supprime le commentaire de la discussion. La ligne reste en base pour les besoins de la modération.
    /// </summary>
    public void MarkDeleted() => Status = CommentStatus.Deleted;

    /// <summary>
    /// Rétablit un commentaire masqué.
    /// </summary>
    public void Republish() => Status = CommentStatus.Published;

    private static Comment Create(string body, Guid authorId, Guid? parentCommentId)
    {
        DomainGuard.AgainstEmpty(authorId, nameof(authorId));

        var text = DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(body, nameof(body)),
            FieldLimits.Body,
            nameof(body));

        return new Comment(text, authorId, parentCommentId);
    }
}
