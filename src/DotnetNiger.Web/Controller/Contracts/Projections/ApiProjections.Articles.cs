using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Infrastructure.Queries;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Projets internes, pour la construction des réponses.</summary>
internal static partial class ApiProjections
{
    /// <summary>
    /// Projette un article en résumé.
    /// </summary>
    /// <param name="article">Article à projeter.</param>
    /// <param name="auteur">Nom de l'auteur, résolu hors requête.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ArticleSummaryResponse ToSummary(Article article, string auteur) =>
        new(article.Id, article.Slug, article.Title, article.Excerpt, article.RubriqueId, article.Rubrique.Nom, auteur, article.PublishedAtUtc, article.CoverImageUrl);

    /// <summary>
    /// Projette un article en résumé d'administration, état de publication compris.
    /// </summary>
    /// <param name="article">Article à projeter.</param>
    /// <param name="auteur">Nom de l'auteur, résolu hors requête.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static AdminArticleSummaryResponse ToAdminSummary(Article article, string auteur) =>
        new(
            article.Id,
            article.Slug,
            article.Title,
            article.Excerpt,
            article.RubriqueId,
            article.Rubrique.Nom,
            article.Status,
            auteur,
            article.PublishedAtUtc,
            article.UpdatedAtUtc);

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
            article.RubriqueId,
            article.Rubrique.Nom,
            article.CoverImageUrl,
            auteur,
            article.PublishedAtUtc);

    /// <summary>
    /// Projette un article lu en base en résumé.
    /// </summary>
    /// <param name="article">Article à projeter.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ArticleSummaryResponse ToSummary(ArticleLu article) =>
        new(
            article.Id,
            article.Slug,
            article.Titre,
            article.Extrait,
            article.RubriqueId,
            article.Rubrique,
            article.Auteur,
            article.PublieLeUtc,
            article.ImageUrl);

    /// <summary>
    /// Projette un article lu en base en réponse complète.
    /// </summary>
    /// <param name="article">Article à projeter.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static ArticleResponse ToDetail(ArticleLu article) =>
        new(
            article.Id,
            article.Slug,
            article.Titre,
            article.Extrait,
            article.Contenu,
            article.RubriqueId,
            article.Rubrique,
            article.ImageUrl,
            article.Auteur,
            article.PublieLeUtc);

}
