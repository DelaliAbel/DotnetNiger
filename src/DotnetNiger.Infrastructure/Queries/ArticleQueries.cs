using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Queries;

/// <summary>Article publié, tel que le sert l'API et le lit le site public.</summary>
/// <param name="Id">Identifiant de l'article.</param>
/// <param name="Slug">Adresse de la fiche.</param>
/// <param name="Titre">Titre.</param>
/// <param name="Extrait">Chapô.</param>
/// <param name="Contenu">Corps, un paragraphe par bloc.</param>
/// <param name="RubriqueId">Identifiant de la rubrique.</param>
/// <param name="Rubrique">Libellé de la rubrique.</param>
/// <param name="ImageUrl">Image de couverture.</param>
/// <param name="AuteurId">Identifiant de l'auteur.</param>
/// <param name="Auteur">Nom affiché de l'auteur.</param>
/// <param name="FonctionAuteur">Fonction professionnelle de l'auteur.</param>
/// <param name="PublieLeUtc">Date de publication.</param>
public sealed record ArticleLu(
    Guid Id,
    string Slug,
    string Titre,
    string Extrait,
    string Contenu,
    Guid RubriqueId,
    string Rubrique,
    string? ImageUrl,
    Guid AuteurId,
    string Auteur,
    string? FonctionAuteur,
    DateTime? PublieLeUtc);

/// <summary>Lecture des articles publiés.</summary>
/// <remarks>
/// Les points de terminaison et les pages publiques passent tous deux par ce service :
/// l'API et le site affichent donc nécessairement les mêmes articles, avec les mêmes
/// tris et les mêmes filtres. Deux implémentations séparées laisseraient un jour une page
/// annoncer un article que l'API ne renvoie pas.
/// <para>
/// Le service vit en infrastructure, et non en application : filtrer et trier suppose de
/// traduire une requête LINQ, ce que la couche application ne peut pas faire sans
/// connaître le moteur d'enregistrement.
/// </para>
/// </remarks>
public sealed class ArticleQueries(IUnitOfWork uniteDeTravail, IMemberDirectory annuaire)
{
    /// <summary>Articles publiés, filtrés et triés.</summary>
    /// <param name="rubrique">Ne garder que cette rubrique, si elle est indiquée.</param>
    /// <param name="recherche">Texte cherché dans le titre et le chapô.</param>
    /// <param name="trier">Ordre demandé : « ancien », « titre », « titre-desc », sinon le plus récent d'abord.</param>
    /// <param name="page">Page demandée, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les articles de la page demandée, et le nombre total correspondant.</returns>
    public async Task<(IReadOnlyList<ArticleLu> Articles, int Total)> ListerAsync(
        Guid? rubrique,
        string? recherche,
        string? trier,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // La rubrique est incluse : la projection renvoie son libellé, et une navigation
        // non chargée vaudrait null.
        var requete = uniteDeTravail.ReadRepository<Article>().Query
            .Include(article => article.Rubrique)
            .Where(article => article.Status == PublicationStatus.Published);

        if (rubrique is { } valeur)
        {
            requete = requete.Where(article => article.RubriqueId == valeur);
        }

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var terme = recherche.Trim();
            requete = requete.Where(article =>
                article.Title.Contains(terme) || article.Excerpt.Contains(terme));
        }

        requete = trier switch
        {
            "ancien" => requete
                .OrderBy(article => article.PublishedAtUtc)
                .ThenBy(article => article.Title),
            "titre" => requete.OrderBy(article => article.Title),
            "titre-desc" => requete.OrderByDescending(article => article.Title),
            _ => requete
                .OrderByDescending(article => article.PublishedAtUtc)
                .ThenBy(article => article.Title),
        };

        var total = await requete.CountAsync(cancellationToken);

        var articles = await requete
            .Skip((Math.Max(page, 1) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var auteurs = await annuaire.ObtenirPlusieursAsync(
            [.. articles.Select(article => article.AuthorId).Distinct()],
            cancellationToken);

        return ([.. articles.Select(article => new ArticleLu(
            article.Id,
            article.Slug,
            article.Title,
            article.Excerpt,
            article.Content,
            article.RubriqueId,
            article.Rubrique.Nom,
            article.CoverImageUrl,
            article.AuthorId,
            auteurs.GetValueOrDefault(article.AuthorId)?.FullName ?? string.Empty,
            auteurs.GetValueOrDefault(article.AuthorId)?.Profession,
            article.PublishedAtUtc))], total);
    }

    /// <summary>Un article publié, par son slug.</summary>
    /// <param name="slug">Slug de l'article.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'article, ou <see langword="null"/> s'il n'existe pas ou n'est pas publié.</returns>
    public async Task<ArticleLu?> LireAsync(string slug, CancellationToken cancellationToken = default)
    {
        var article = await uniteDeTravail.ReadRepository<Article>().Query
            .Include(candidat => candidat.Rubrique)
            .FirstOrDefaultAsync(
                candidat => candidat.Slug == slug && candidat.Status == PublicationStatus.Published,
                cancellationToken);

        if (article is null)
        {
            return null;
        }

        var auteurs = await annuaire.ObtenirPlusieursAsync([article.AuthorId], cancellationToken);

        return new ArticleLu(
            article.Id,
            article.Slug,
            article.Title,
            article.Excerpt,
            article.Content,
            article.RubriqueId,
            article.Rubrique.Nom,
            article.CoverImageUrl,
            article.AuthorId,
            auteurs.GetValueOrDefault(article.AuthorId)?.FullName ?? string.Empty,
            auteurs.GetValueOrDefault(article.AuthorId)?.Profession,
            article.PublishedAtUtc);
    }

}
