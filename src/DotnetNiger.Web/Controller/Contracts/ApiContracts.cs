using System.ComponentModel.DataAnnotations;
using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Moderation;
using DotnetNiger.Domain.Entities.Partners;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Identity;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>
/// Projets de l'API publique.
/// </summary>
public static class ApiRoutes
{
    /// <summary>
    /// Préfixe commun des routes. Aucun versionnement n'est appliqué : le contrat est en
    /// construction et n'a pas encore été publié à des clients qu'il serait coûteux de
    /// rompre.
    /// </summary>
    public const string Base = "/api";
}

/// <summary>
/// Paramètres de pagination acceptés par les listes.
/// </summary>
/// <param name="Page">Numéro de page, à partir de 1.</param>
/// <param name="PageSize">Nombre d'éléments par page.</param>
public readonly record struct PageQuery(int Page = 1, int PageSize = 20);

/// <summary>
/// Page de résultats exposée par l'API.
/// </summary>
/// <typeparam name="T">Type des éléments.</typeparam>
/// <param name="Items">Éléments de la page.</param>
/// <param name="Page">Numéro de page.</param>
/// <param name="PageSize">Taille de page.</param>
/// <param name="TotalCount">Nombre total d'éléments correspondant au filtre.</param>
/// <param name="TotalPages">Nombre total de pages.</param>
/// <param name="HasNext">Vrai s'il existe une page suivante.</param>
public sealed record PageResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNext);

/// <summary>Résumé d'un article publié.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Author">Nom de l'auteur.</param>
/// <param name="PublishedAtUtc">Date de publication.</param>
public sealed record ArticleSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    ArticleCategory Category,
    string Author,
    DateTime? PublishedAtUtc);

/// <summary>Article complet.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="Content">Contenu rédigé.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="CoverImageUrl">Image de couverture.</param>
/// <param name="Author">Nom de l'auteur.</param>
/// <param name="PublishedAtUtc">Date de publication.</param>
public sealed record ArticleResponse(
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    string Content,
    ArticleCategory Category,
    string? CoverImageUrl,
    string Author,
    DateTime? PublishedAtUtc);

/// <summary>Demande de création d'un article.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="Content">Contenu rédigé.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Publish">Publier immédiatement plutôt que garder en brouillon.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette du blog.</param>
public sealed record CreateArticleRequest(
    [property: Required, StringLength(200)] string Title,
    [property: Required, StringLength(500)] string Excerpt,
    [property: Required] string Content,
    ArticleCategory Category,
    bool Publish = false,
    [property: StringLength(2048)] string? CoverImageUrl = null);

/// <summary>Résumé d'une ressource.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Provider">Éditeur ou source.</param>
/// <param name="ExternalUrl">URL externe, si la ressource est un lien.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record ResourceSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Description,
    ResourceCategory Category,
    string? Provider,
    string? ExternalUrl,
    string? CoverImageUrl);

/// <summary>Demande de création d'une ressource.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="ExternalUrl">URL de la ressource externe, si applicable.</param>
/// <param name="Provider">Éditeur ou source.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record CreateResourceRequest(
    [property: Required, StringLength(200)] string Title,
    [property: Required, StringLength(500)] string Description,
    ResourceCategory Category,
    [property: StringLength(2048)] string? ExternalUrl = null,
    [property: StringLength(120)] string? Provider = null,
    [property: StringLength(2048)] string? CoverImageUrl = null);

/// <summary>Résumé d'un événement.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="StartsAtUtc">Début.</param>
/// <param name="EndsAtUtc">Fin.</param>
/// <param name="Location">Lieu, pour un événement en présentiel.</param>
/// <param name="OnlineUrl">Lien de participation, en ligne.</param>
/// <param name="Capacity">Nombre de places, si limité.</param>
/// <param name="ConfirmedCount">Inscriptions confirmées.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record EventSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    CommunityEventFormat Format,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    string? OnlineUrl,
    int? Capacity,
    int ConfirmedCount,
    string? CoverImageUrl);

/// <summary>Demande d'inscription à un événement.</summary>
/// <param name="EventId">Identifiant de l'événement.</param>
public sealed record RegisterForEventRequest(
    [property: Required] Guid EventId);

/// <summary>Résumé d'un projet.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Name">Nom du projet.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Technologies">Technologies utilisées.</param>
/// <param name="RepositoryUrl">Dépôt.</param>
/// <param name="ImageUrl">Image de présentation, pour la vignette.</param>
public sealed record ProjectSummaryResponse(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string? Technologies,
    string RepositoryUrl,
    string? ImageUrl);

/// <summary>Demande de création d'un projet.</summary>
/// <param name="Name">Nom du projet.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Description">Description détaillée.</param>
/// <param name="RepositoryUrl">Dépôt.</param>
/// <param name="Technologies">Technologies utilisées.</param>
/// <param name="ImageUrl">Image de présentation, pour la vignette.</param>
public sealed record CreateProjectRequest(
    [property: Required, StringLength(160)] string Name,
    [property: Required, StringLength(500)] string Summary,
    [property: Required] string Description,
    [property: Required, StringLength(2048)] string RepositoryUrl,
    [property: StringLength(500)] string? Technologies = null,
    [property: StringLength(2048)] string? ImageUrl = null);

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
    [property: Required, StringLength(4000)] string Body,
    Guid? ArticleId = null,
    Guid? EventId = null,
    Guid? ParentCommentId = null);

/// <summary>Profil public d'un membre.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug du profil.</param>
/// <param name="FullName">Nom complet.</param>
/// <param name="Bio">Biographie.</param>
/// <param name="City">Ville.</param>
/// <param name="PhotoUrl">Photo de profil.</param>
/// <param name="Technologies">Compétences déclarées.</param>
/// <param name="IsTeam">Membre de l'équipe organisatrice.</param>
public sealed record MemberResponse(
    Guid Id,
    string? Slug,
    string FullName,
    string? Bio,
    string? City,
    string? PhotoUrl,
    string? Technologies,
    bool IsTeam);

/// <summary>Partenaire affiché sur le site.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Name">Nom.</param>
/// <param name="Description">Description.</param>
/// <param name="WebsiteUrl">Site web.</param>
/// <param name="LogoUrl">Logo.</param>
/// <param name="Tier">Niveau de partenariat.</param>
public sealed record PartnerResponse(
    Guid Id,
    string Name,
    string Description,
    string WebsiteUrl,
    string? LogoUrl,
    PartnerTier Tier);

/// <summary>Signalement de contenu.</summary>
/// <param name="Reason">Motif du signalement.</param>
/// <param name="Details">Précisions facultatives.</param>
public sealed record CreateReportRequest(
    ReportReason Reason,
    [property: StringLength(1000)] string? Details = null);

/// <summary>État d'un signalement, réservé à la modération.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="TargetType">Type de cible.</param>
/// <param name="TargetId">Identifiant de la cible.</param>
/// <param name="Reason">Motif.</param>
/// <param name="Status">État de traitement.</param>
/// <param name="CreatedAtUtc">Date de création.</param>
public sealed record ReportResponse(
    Guid Id,
    ReportTargetType TargetType,
    Guid TargetId,
    ReportReason Reason,
    ReportStatus Status,
    DateTime CreatedAtUtc);

/// <summary>Projets internes, pour la construction des réponses.</summary>
internal static class ApiProjections
{
    /// <summary>
    /// Projette un article en résumé.
    /// </summary>
    /// <param name="article">Article à projeter.</param>
    /// <param name="auteur">Nom de l'auteur, résolu hors requête.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ArticleSummaryResponse ToSummary(Article article, string auteur) =>
        new(article.Id, article.Slug, article.Title, article.Excerpt, article.Category, auteur, article.PublishedAtUtc);

    /// <summary>
    /// Projette un article en réponse complète.
    /// </summary>
    /// <param name="article">Article à projeter.</param>
    /// <param name="auteur">Nom de l'auteur, résolu hors requête.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static ArticleResponse ToDetail(Article article, string auteur) =>
        new(
            article.Id,
            article.Slug,
            article.Title,
            article.Excerpt,
            article.Content,
            article.Category,
            article.CoverImageUrl,
            auteur,
            article.PublishedAtUtc);

    /// <summary>
    /// Projette une ressource en résumé.
    /// </summary>
    /// <param name="resource">Ressource à projeter.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ResourceSummaryResponse ToSummary(Resource resource) =>
        new(
            resource.Id,
            resource.Slug,
            resource.Title,
            resource.Description,
            resource.Category,
            resource.Provider,
            resource.ExternalUrl,
            resource.CoverImageUrl);

    /// <summary>
    /// Projette un événement en résumé.
    /// </summary>
    /// <param name="communityEvent">Événement à projeter.</param>
    /// <param name="confirmes">Nombre d'inscriptions confirmées.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static EventSummaryResponse ToSummary(CommunityEvent communityEvent, int confirmes) =>
        new(
            communityEvent.Id,
            communityEvent.Slug,
            communityEvent.Title,
            communityEvent.Summary,
            communityEvent.Format,
            communityEvent.StartsAtUtc,
            communityEvent.EndsAtUtc,
            communityEvent.Location,
            communityEvent.OnlineUrl,
            communityEvent.Capacity,
            confirmes,
            communityEvent.CoverImageUrl);

    /// <summary>
    /// Projette un projet en résumé.
    /// </summary>
    /// <param name="project">Projet à projeter.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ProjectSummaryResponse ToSummary(Project project) =>
        new(
            project.Id,
            project.Slug,
            project.Name,
            project.Summary,
            project.Technologies,
            project.RepositoryUrl,
            project.ImageUrl);

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
    /// Projette un partenaire.
    /// </summary>
    /// <param name="partner">Partenaire à projeter.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static PartnerResponse ToResponse(Partner partner) =>
        new(
            partner.Id,
            partner.Name,
            partner.Description,
            partner.WebsiteUrl,
            partner.LogoUrl,
            partner.Tier);

    /// <summary>
    /// Projette un signalement pour la modération.
    /// </summary>
    /// <param name="report">Signalement à projeter.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static ReportResponse ToResponse(Report report) =>
        new(report.Id, report.TargetType, report.TargetId, report.Reason, report.Status, report.CreatedAtUtc);

    /// <summary>
    /// Construit une page de réponses à partir d'un résultat paginé du domaine.
    /// </summary>
    /// <typeparam name="TSource">Type des entités.</typeparam>
    /// <typeparam name="TResult">Type des réponses.</typeparam>
    /// <param name="result">Résultat paginé.</param>
    /// <param name="selector">Projection appliquée à chaque entité.</param>
    /// <returns>Page sérialisable.</returns>
    public static PageResponse<TResult> ToPage<TSource, TResult>(
        Application.Common.PagedResult<TSource> result,
        Func<TSource, TResult> selector) =>
        new(
            [.. result.Items.Select(selector)],
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasNext);
}
