using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Un article, tel que le liste l'administration.</summary>
/// <param name="Id">Identifiant de l'article.</param>
/// <param name="Slug">Adresse de la fiche.</param>
/// <param name="Titre">Titre.</param>
/// <param name="Chapo">Chapô.</param>
/// <param name="RubriqueId">Identifiant de la rubrique.</param>
/// <param name="Rubrique">Libellé de la rubrique.</param>
/// <param name="Statut">État de publication.</param>
/// <param name="Auteur">Nom de l'auteur.</param>
/// <param name="PublieLe">Date de publication, ou <see langword="null"/> si jamais publié.</param>
/// <param name="ModifieLe">Dernière modification, ou <see langword="null"/> si intact.</param>
public sealed record LigneArticleAdmin(
    Guid Id,
    string Slug,
    string Titre,
    string Chapo,
    Guid RubriqueId,
    string Rubrique,
    PublicationStatus Statut,
    string Auteur,
    DateTime? PublieLe,
    DateTime? ModifieLe);

/// <summary>Une rubrique, proposée dans le menu déroulant de l'éditeur.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Nom">Libellé.</param>
public sealed record RubriqueOptionVue(Guid Id, string Nom);

/// <summary>Liste complète lue pour la page Blog.</summary>
/// <param name="Articles">Articles, le plus récemment modifié d'abord.</param>
/// <param name="Rubriques">Rubriques du blog, par ordre alphabétique.</param>
public sealed record BlogVue(
    IReadOnlyList<LigneArticleAdmin> Articles,
    IReadOnlyList<RubriqueOptionVue> Rubriques);

/// <summary>Article complet, pour la reprise dans l'éditeur.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Titre">Titre.</param>
/// <param name="Chapo">Chapô.</param>
/// <param name="Contenu">Corps au format HTML.</param>
/// <param name="RubriqueId">Identifiant de la rubrique.</param>
/// <param name="ImageUrl">Image de couverture, ou <see langword="null"/>.</param>
public sealed record ArticleAdminDetail(
    Guid Id,
    string Titre,
    string Chapo,
    string Contenu,
    Guid RubriqueId,
    string? ImageUrl);

/// <summary>
/// Lecture des articles pour l'administration : la liste, ses rubriques, et la reprise
/// d'un article dans l'éditeur. Les écritures, elles, passent par les points d'extrémité
/// de l'API : ce service ne lit que ce que la base sait déjà.
/// </summary>
/// <remarks>
/// La lecture est mémoïsée, comme au tableau de bord : la liste ne change que par une
/// écriture, et chaque écriture appelle <see cref="Oublie"/> avant de relire.
/// </remarks>
public sealed class ServiceBlog(IUnitOfWork uniteDeTravail, IMemberDirectory annuaire)
{
    private Task<BlogVue>? _lecture;

    /// <summary>
    /// Lit la liste des articles et les rubriques, une seule fois par changement.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Articles et rubriques.</returns>
    public Task<BlogVue> LireAsync(CancellationToken cancellationToken = default)
        => (_lecture ??= LireInterneAsync(cancellationToken));

    /// <summary>
    /// Lit un article en entier, pour l'ouvrir dans l'éditeur.
    /// </summary>
    /// <param name="articleId">Identifiant de l'article.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le contenu, ou <see langword="null"/> si l'article est introuvable.</returns>
    public async Task<ArticleAdminDetail?> LireDetailAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        var article = await uniteDeTravail.ReadRepository<Article>().Query
            .AsNoTracking()
            .FirstOrDefaultAsync(candidat => candidat.Id == articleId, cancellationToken);

        return article is null
            ? null
            : new ArticleAdminDetail(
                article.Id,
                article.Title,
                article.Excerpt,
                article.Content,
                article.RubriqueId,
                article.CoverImageUrl);
    }

    /// <summary>Relit la liste à la prochaine demande, après une écriture.</summary>
    public void Oublie() => _lecture = null;

    private async Task<BlogVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        var articles = await uniteDeTravail.ReadRepository<Article>().Query
            .AsNoTracking()
            .Include(candidat => candidat.Rubrique)
            .OrderByDescending(candidat => candidat.UpdatedAtUtc ?? candidat.CreatedAtUtc)
            .ThenBy(candidat => candidat.Title)
            .ToListAsync(cancellationToken);

        var auteurs = await annuaire.ObtenirPlusieursAsync(
            [.. articles.Select(candidat => candidat.AuthorId).Distinct()],
            cancellationToken);

        var rubriques = await uniteDeTravail.ReadRepository<Rubrique>().Query
            .AsNoTracking()
            .OrderBy(rubrique => rubrique.Nom)
            .Select(rubrique => new RubriqueOptionVue(rubrique.Id, rubrique.Nom))
            .ToListAsync(cancellationToken);

        return new BlogVue(
            [.. articles.Select(candidat => new LigneArticleAdmin(
                candidat.Id,
                candidat.Slug,
                candidat.Title,
                candidat.Excerpt,
                candidat.RubriqueId,
                candidat.Rubrique.Nom,
                candidat.Status,
                auteurs.GetValueOrDefault(candidat.AuthorId)?.FullName ?? string.Empty,
                candidat.PublishedAtUtc,
                candidat.UpdatedAtUtc))],
            rubriques);
    }
}