using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Nature de la cible d'un commentaire.</summary>
/// <param name="EstArticle">Vrai pour un article de blog, faux pour un événement.</param>
/// <param name="Libelle">Intitulé affiché.</param>
/// <param name="Lien">Adresse de la fiche publique, ou vide si la cible a disparu.</param>
public sealed record CibleCommentaire(bool EstArticle, string Libelle, string Lien);

/// <summary>Un commentaire, tel que la modération le voit.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Corps">Texte déposé par le membre.</param>
/// <param name="Auteur">Nom du membre auteur.</param>
/// <param name="EstReponse">Vrai si le commentaire répond à un autre commentaire.</param>
/// <param name="Cible">Article ou événement commenté.</param>
/// <param name="Statut">État de modération.</param>
/// <param name="DeposeLe">Date de dépôt (UTC).</param>
public sealed record LigneCommentaireAdmin(
    Guid Id,
    string Corps,
    string Auteur,
    bool EstReponse,
    CibleCommentaire Cible,
    CommentStatus Statut,
    DateTime DeposeLe);

/// <summary>Tous les commentaires, lus pour la page Commentaires.</summary>
/// <param name="Commentaires">Commentaires, du plus récent au plus ancien.</param>
public sealed record CommentairesVue(IReadOnlyList<LigneCommentaireAdmin> Commentaires);

/// <summary>
/// Lecture des commentaires pour la modération. Les écritures passent par les points
/// d'extrémité de l'API, comme pour le blog et les événements.
/// </summary>
/// <remarks>
/// Contrairement à la liste publique, celle-ci ne filtre aucun état. Un commentaire masqué
/// ou mis de côté doit rester visible : c'est la seule façon de savoir ce qui a été sanctionné,
/// et de le rétablir si la sanction était une erreur.
/// </remarks>
public sealed class ServiceCommentaires(IUnitOfWork uniteDeTravail, IMemberDirectory annuaire)
{
    private Task<CommentairesVue>? _lecture;

    /// <summary>Lit tous les commentaires, une seule fois par changement.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les commentaires de la modération.</returns>
    public Task<CommentairesVue> LireAsync(CancellationToken cancellationToken = default)
        => (_lecture ??= LireInterneAsync(cancellationToken));

    /// <summary>Relit la liste à la prochaine demande, après une écriture.</summary>
    public void Oublie() => _lecture = null;

    private async Task<CommentairesVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        var commentaires = await uniteDeTravail.ReadRepository<Comment>().Query
            .AsNoTracking()
            .OrderByDescending(comment => comment.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        if (commentaires.Count is 0)
        {
            return new CommentairesVue([]);
        }

        var auteurs = await annuaire.ObtenirPlusieursAsync(
            [.. commentaires.Select(candidat => candidat.AuthorId).Distinct()],
            cancellationToken);

        // Les cibles sont lues séparément, et uniquement pour les types réellement présents :
        // une file de modération affiche surtout des commentaires, et charger tous les
        // événements pour une page qui n'en contient aucun serait une lecture pour rien.
        var articles = await CiblesArticlesAsync(
            [.. commentaires.Where(comment => comment.ArticleId is not null)
                .Select(comment => comment.ArticleId!.Value)],
            cancellationToken);

        var evenements = await CiblesEvenementsAsync(
            [.. commentaires.Where(comment => comment.CommunityEventId is not null)
                .Select(comment => comment.CommunityEventId!.Value)],
            cancellationToken);

        return new CommentairesVue(
            [.. commentaires.Select(commentaire => new LigneCommentaireAdmin(
                commentaire.Id,
                commentaire.Body,
                auteurs.GetValueOrDefault(commentaire.AuthorId)?.FullName ?? "Membre supprimé",
                commentaire.ParentCommentId is not null,
                Cible(commentaire, articles, evenements),
                commentaire.Status,
                commentaire.CreatedAtUtc))]);
    }

    /// <summary>
    /// Décrit la cible d'un commentaire : un article de blog ou un événement.
    /// </summary>
    /// <param name="commentaire">Commentaire concerné.</param>
    /// <param name="articles">Articles commentés, par identifiant.</param>
    /// <param name="evenements">Événements commentés, par identifiant.</param>
    /// <returns>La cible, avec son adresse publique.</returns>
    /// <remarks>
    /// Une cible disparue — article retiré, événement supprimé — reste décrite par son type.
    /// Le fil avait existé, et « cible inconnue » ferait croire à une erreur de lecture.
    /// </remarks>
    private static CibleCommentaire Cible(
        Comment commentaire,
        IReadOnlyDictionary<Guid, Fiche> articles,
        IReadOnlyDictionary<Guid, Fiche> evenements)
    {
        if (commentaire.ArticleId is { } idArticle)
        {
            return Decrire(articles.GetValueOrDefault(idArticle), estArticle: true, "Article retiré", "/blog");
        }

        if (commentaire.CommunityEventId is { } idEvenement)
        {
            return Decrire(evenements.GetValueOrDefault(idEvenement), estArticle: false, "Événement retiré", "/evenements");
        }

        return new CibleCommentaire(EstArticle: false, Libelle: "Cible inconnue", Lien: string.Empty);
    }

    private static CibleCommentaire Decrire(Fiche? fiche, bool estArticle, string disparu, string racine) =>
        fiche is null
            ? new CibleCommentaire(estArticle, disparu, string.Empty)
            : new CibleCommentaire(estArticle, fiche.Titre, $"{racine}/{fiche.Slug}");

    private async Task<IReadOnlyDictionary<Guid, Fiche>> CiblesArticlesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken) =>
        await uniteDeTravail.ReadRepository<Article>().Query
            .AsNoTracking()
            .Where(article => ids.Contains(article.Id))
            .Select(article => new Fiche(article.Id, article.Title, article.Slug))
            .ToDictionaryAsync(fiche => fiche.Id, cancellationToken);

    private async Task<IReadOnlyDictionary<Guid, Fiche>> CiblesEvenementsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken) =>
        await uniteDeTravail.ReadRepository<CommunityEvent>().Query
            .AsNoTracking()
            .Where(evenement => ids.Contains(evenement.Id))
            .Select(evenement => new Fiche(evenement.Id, evenement.Title, evenement.Slug))
            .ToDictionaryAsync(fiche => fiche.Id, cancellationToken);

    /// <summary>Titre et adresse d'un contenu commenté.</summary>
    /// <param name="Id">Identifiant.</param>
    /// <param name="Titre">Titre affiché.</param>
    /// <param name="Slug">Adresse de la fiche.</param>
    private sealed record Fiche(Guid Id, string Titre, string Slug);
}