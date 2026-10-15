using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Entities.Moderation;
using DotnetNiger.Infrastructure.Identity;

namespace DotnetNiger.Web.Controller.Contracts;

internal static partial class ApiProjections
{
    /// <summary>
    /// Projette un commentaire publié.
    /// </summary>
    /// <param name="comment">Commentaire à projeter.</param>
    /// <param name="auteur">Nom de l'auteur, résolu hors requête.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static CommentResponse ToResponse(Comment comment, string auteur) =>
        new(
            comment.Id,
            comment.ArticleId,
            comment.CommunityEventId,
            comment.ParentCommentId,
            comment.Body,
            auteur,
            comment.CreatedAtUtc);

    /// <summary>
    /// Projette un profil public de membre.
    /// </summary>
    /// <param name="user">Membre à projeter.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static MemberResponse ToResponse(ApplicationUser user) =>
        new(
            user.Id,
            user.Slug,
            user.FullName,
            user.Bio,
            user.City,
            user.PhotoUrl,
            user.Technologies,
            user.IsTeam);

    /// <summary>
    /// Projette un signalement pour la modération.
    /// </summary>
    /// <param name="report">Signalement à projeter.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static ReportResponse ToResponse(Report report) =>
        new(report.Id, report.TargetType, report.TargetId, report.Reason, report.Status, report.CreatedAtUtc);

}
