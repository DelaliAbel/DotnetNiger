using System.ComponentModel.DataAnnotations;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Résumé d'un article publié.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="CategoryId">Identifiant de la rubrique.</param>
/// <param name="Category">Libellé de la rubrique.</param>
/// <param name="Author">Nom de l'auteur.</param>
/// <param name="PublishedAtUtc">Date de publication.</param>
/// <param name="CoverImageUrl">
/// Image de couverture, absente d'un article sans illustration.
/// </param>
/// <remarks>
/// Le résumé et l'article complet ne peuvent pas diverger sur ce point : l'un alimente les
/// cartes de la liste du blog, l'autre la page de partage. Une couverture visible sur la
/// fiche mais absente de la carte — ou l'inverse — ferait croire à un article sans image.
/// </remarks>
public sealed record ArticleSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    Guid CategoryId,
    string Category,
    string Author,
    DateTime? PublishedAtUtc,
    string? CoverImageUrl = null);

/// <summary>Article complet.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="Content">Contenu rédigé.</param>
/// <param name="CategoryId">Identifiant de la rubrique.</param>
/// <param name="Category">Libellé de la rubrique.</param>
/// <param name="CoverImageUrl">Image de couverture.</param>
/// <param name="Author">Nom de l'auteur.</param>
/// <param name="PublishedAtUtc">Date de publication.</param>
public sealed record ArticleResponse(
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    string Content,
    Guid CategoryId,
    string Category,
    string? CoverImageUrl,
    string Author,
    DateTime? PublishedAtUtc);

/// <summary>Article tel que l'administration le voit : tous les états, pas seulement le publié.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="CategoryId">Identifiant de la rubrique.</param>
/// <param name="Category">Libellé de la rubrique.</param>
/// <param name="Status">État de publication.</param>
/// <param name="Author">Nom de l'auteur.</param>
/// <param name="PublishedAtUtc">Date de publication.</param>
/// <param name="UpdatedAtUtc">Dernière modification, ou <see langword="null"/> si l'article est resté intact depuis sa création.</param>
public sealed record AdminArticleSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    Guid CategoryId,
    string Category,
    PublicationStatus Status,
    string Author,
    DateTime? PublishedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>Demande de création d'un article.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="Content">Contenu rédigé.</param>
/// <param name="CategoryId">Identifiant de la rubrique.</param>
/// <param name="Publish">Publier immédiatement plutôt que garder en brouillon.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette du blog.</param>
public sealed record CreateArticleRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Excerpt,
    [Required] string Content,
    Guid CategoryId,
    bool Publish = false,
    [StringLength(2048)] string? CoverImageUrl = null);

/// <summary>Demande de mise à jour d'un article.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="Content">Contenu rédigé.</param>
/// <param name="CategoryId">Identifiant de la rubrique.</param>
/// <param name="CoverImageUrl">Image de couverture, ou <see langword="null"/> pour la retirer.</param>
public sealed record UpdateArticleRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Excerpt,
    [Required] string Content,
    Guid CategoryId,
    [StringLength(2048)] string? CoverImageUrl = null);

