using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Moderation;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Le contenu visé par un signalement.</summary>
/// <param name="Titre">Intitulé du contenu, ou la nature de la cible si elle a disparu.</param>
/// <param name="Lien">Adresse de la fiche publique, ou vide si la cible a disparu.</param>
/// <param name="Extrait">Extrait du texte, pour un commentaire.</param>
public sealed record CibleSignalee(string Titre, string Lien, string? Extrait);

/// <summary>Un signalement, tel que la modération le voit.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Type">Nature du contenu signalé.</param>
/// <param name="CibleId">Identifiant du contenu signalé, pour agir dessus.</param>
/// <param name="Motif">Motif donné par le membre.</param>
/// <param name="Details">Précisions fournies, ou <see langword="null"/>.</param>
/// <param name="Cible">Contenu signalé.</param>
/// <param name="Rapporteur">Membre ayant signalé.</param>
/// <param name="Statut">État de traitement.</param>
/// <param name="TraitePar">Administrateur ayant pris en charge, ou <see langword="null"/>.</param>
/// <param name="Conclusion">Note de clôture, ou <see langword="null"/>.</param>
/// <param name="DeposeLe">Date de dépôt (UTC).</param>
/// <param name="ClotureLe">Date de clôture (UTC), ou <see langword="null"/>.</param>
public sealed record LigneSignalementAdmin(
    Guid Id,
    ReportTargetType Type,
    Guid CibleId,
    ReportReason Motif,
    string? Details,
    CibleSignalee Cible,
    string Rapporteur,
    ReportStatus Statut,
    string? TraitePar,
    string? Conclusion,
    DateTime DeposeLe,
    DateTime? ClotureLe);

/// <summary>Tous les signalements, lus pour la page Signalements.</summary>
/// <param name="Signalements">Signalements, les plus anciens d'abord.</param>
public sealed record SignalementsVue(IReadOnlyList<LigneSignalementAdmin> Signalements);

/// <summary>
/// Lecture des signalements pour la modération. Les écritures passent par les points
/// d'extermination de l'API.
/// </summary>
/// <remarks>
/// Un signalement ne porte qu'un identifiant de cible, jamais le contenu lui-même : sans
/// aller le chercher, la file de modération ne montre qu'une liste d'identifiants, et le
/// modérateur ne peut rien décider. Le contenu est donc résolu à la lecture, par type.
/// </remarks>
public sealed class ServiceSignalements(IUnitOfWork uniteDeTravail, IMemberDirectory annuaire)
{
    private Task<SignalementsVue>? _lecture;

    /// <summary>Lit les signalements, une seule fois par changement.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les signalements de la modération.</returns>
    public Task<SignalementsVue> LireAsync(CancellationToken cancellationToken = default)
        => (_lecture ??= LireInterneAsync(cancellationToken));

    /// <summary>Relit la liste à la prochaine demande, après une écriture.</summary>
    public void Oublie() => _lecture = null;

    private async Task<SignalementsVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        // Les plus anciens d'abord : un signalement attend une file de modération, et le
        // plus ancien est celui qui attend depuis le plus longtemps.
        var signalements = await uniteDeTravail.ReadRepository<Report>().Query
            .AsNoTracking()
            .OrderBy(signalement => signalement.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        if (signalements.Count is 0)
        {
            return new SignalementsVue([]);
        }

        var cibles = await ResoudreCiblesAsync(signalements, cancellationToken);

        var personnes = await annuaire.ObtenirPlusieursAsync(
            [.. signalements.SelectMany(signalement => new[] { signalement.ReporterId, signalement.HandledById })
                .OfType<Guid>()
                .Distinct()],
            cancellationToken);

        return new SignalementsVue(
            [.. signalements.Select(signalement => new LigneSignalementAdmin(
                signalement.Id,
                signalement.TargetType,
                signalement.TargetId,
                signalement.Reason,
                signalement.Details,
                Cible(signalement.TargetType, signalement.TargetId, cibles),
                personnes.GetValueOrDefault(signalement.ReporterId)?.FullName ?? "Membre supprimé",
                signalement.Status,
                signalement.HandledById is { } traiteur
                    ? personnes.GetValueOrDefault(traiteur)?.FullName ?? "Administrateur supprimé"
                    : null,
                signalement.ResolutionNote,
                signalement.CreatedAtUtc,
                signalement.HandledAtUtc))]);
    }

    /// <summary>Résout en une lecture par type le contenu visé par les signalements.</summary>
    /// <param name="signalements">Signalements à traiter.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les contenus, repérés par type puis par identifiant.</returns>
    private async Task<IReadOnlyDictionary<ReportTargetType, IReadOnlyDictionary<Guid, CibleSignalee>>> ResoudreCiblesAsync(
        IReadOnlyCollection<Report> signalements,
        CancellationToken cancellationToken)
    {
        var cibles = new Dictionary<ReportTargetType, IReadOnlyDictionary<Guid, CibleSignalee>>();

        foreach (var type in Enum.GetValues<ReportTargetType>())
        {
            var ids = signalements
                .Where(signalement => signalement.TargetType == type)
                .Select(signalement => signalement.TargetId)
                .Distinct()
                .ToArray();

            if (ids.Length is 0)
            {
                continue;
            }

            cibles[type] = type switch
            {
                ReportTargetType.Article => await ArticlesAsync(ids, cancellationToken),
                ReportTargetType.Comment => await CommentairesAsync(ids, cancellationToken),
                ReportTargetType.Resource => await RessourcesAsync(ids, cancellationToken),
                ReportTargetType.CommunityEvent => await EvenementsAsync(ids, cancellationToken),
                ReportTargetType.Project => await ProjetsAsync(ids, cancellationToken),
                _ => await MembresAsync(ids, cancellationToken),
            };
        }

        return cibles;
    }

    private async Task<IReadOnlyDictionary<Guid, CibleSignalee>> ArticlesAsync(
        Guid[] ids,
        CancellationToken cancellationToken) =>
        await uniteDeTravail.ReadRepository<Article>().Query
            .AsNoTracking()
            .Where(article => ids.Contains(article.Id))
            .Select(article => new { article.Id, article.Title, article.Slug })
            .ToDictionaryAsync(
                article => article.Id,
                article => new CibleSignalee(article.Title, $"/blog/{article.Slug}", null),
                cancellationToken);

    private async Task<IReadOnlyDictionary<Guid, CibleSignalee>> CommentairesAsync(
        Guid[] ids,
        CancellationToken cancellationToken)
    {
        var commentaires = await uniteDeTravail.ReadRepository<Comment>().Query
            .AsNoTracking()
            .Where(commentaire => ids.Contains(commentaire.Id))
            .Select(commentaire => new
            {
                commentaire.Id,
                commentaire.Body,
                commentaire.ArticleId,
                commentaire.CommunityEventId,
            })
            .ToListAsync(cancellationToken);

        // Un commentaire est signalé par son texte, pas par un titre : la cible affichée est
        // donc un extrait, et le fil où il se trouve est résolu dans la foulée.
        var articles = await ArticlesDesCommentairesAsync(
            [.. commentaires.Where(c => c.ArticleId is not null).Select(c => c.ArticleId!.Value)],
            cancellationToken);

        var evenements = await EvenementsDesCommentairesAsync(
            [.. commentaires.Where(c => c.CommunityEventId is not null).Select(c => c.CommunityEventId!.Value)],
            cancellationToken);

        return commentaires.ToDictionary(
            commentaire => commentaire.Id,
            commentaire => new CibleSignalee(
                "Commentaire",
                commentaire.ArticleId is { } idArticle
                    ? $"/blog/{articles.GetValueOrDefault(idArticle)}"
                    : evenements.GetValueOrDefault(commentaire.CommunityEventId!.Value) is { } adresse
                        ? $"/evenements/{adresse}"
                        : string.Empty,
                commentaire.Body));
    }

    private async Task<IReadOnlyDictionary<Guid, string>> ArticlesDesCommentairesAsync(
        Guid[] ids,
        CancellationToken cancellationToken) =>
        ids.Length is 0
            ? new Dictionary<Guid, string>()
            : await uniteDeTravail.ReadRepository<Article>().Query
                .AsNoTracking()
                .Where(article => ids.Contains(article.Id))
                .ToDictionaryAsync(article => article.Id, article => article.Slug, cancellationToken);

    private async Task<IReadOnlyDictionary<Guid, string>> EvenementsDesCommentairesAsync(
        Guid[] ids,
        CancellationToken cancellationToken) =>
        ids.Length is 0
            ? new Dictionary<Guid, string>()
            : await uniteDeTravail.ReadRepository<CommunityEvent>().Query
                .AsNoTracking()
                .Where(evenement => ids.Contains(evenement.Id))
                .ToDictionaryAsync(evenement => evenement.Id, evenement => evenement.Slug, cancellationToken);

    private async Task<IReadOnlyDictionary<Guid, CibleSignalee>> RessourcesAsync(
        Guid[] ids,
        CancellationToken cancellationToken) =>
        // Une ressource n'a pas de fiche : le public n'en montre qu'une carte, sur la page
        // des ressources. C'est donc là que le modérateur la retrouve.
        await uniteDeTravail.ReadRepository<Resource>().Query
            .AsNoTracking()
            .Where(ressource => ids.Contains(ressource.Id))
            .Select(ressource => new { ressource.Id, ressource.Title })
            .ToDictionaryAsync(
                ressource => ressource.Id,
                ressource => new CibleSignalee(ressource.Title, "/ressources", null),
                cancellationToken);

    private async Task<IReadOnlyDictionary<Guid, CibleSignalee>> EvenementsAsync(
        Guid[] ids,
        CancellationToken cancellationToken) =>
        await uniteDeTravail.ReadRepository<CommunityEvent>().Query
            .AsNoTracking()
            .Where(evenement => ids.Contains(evenement.Id))
            .Select(evenement => new { evenement.Id, evenement.Title, evenement.Slug })
            .ToDictionaryAsync(
                evenement => evenement.Id,
                evenement => new CibleSignalee(evenement.Title, $"/evenements/{evenement.Slug}", null),
                cancellationToken);

    private async Task<IReadOnlyDictionary<Guid, CibleSignalee>> ProjetsAsync(
        Guid[] ids,
        CancellationToken cancellationToken) =>
        // Un projet n'a pas de fiche non plus : la liste vit sur la page de la communauté.
        await uniteDeTravail.ReadRepository<Project>().Query
            .AsNoTracking()
            .Where(projet => ids.Contains(projet.Id))
            .Select(projet => new { projet.Id, projet.Name })
            .ToDictionaryAsync(
                projet => projet.Id,
                projet => new CibleSignalee(projet.Name, "/communaute#projets", null),
                cancellationToken);

    private async Task<IReadOnlyDictionary<Guid, CibleSignalee>> MembresAsync(
        Guid[] ids,
        CancellationToken cancellationToken)
    {
        // L'annuaire ne rend qu'un nom, sans adresse de profil : les membres sont donc renvoyés
        // vers la liste de la communauté, où le modérateur cherchera le nom signalé.
        var profils = await annuaire.ObtenirPlusieursAsync(ids, cancellationToken);

        return ids.ToDictionary(
            id => id,
            id => new CibleSignalee(
                profils.GetValueOrDefault(id)?.FullName ?? "Membre introuvable",
                "/communaute",
                null));
    }

    /// <summary>Décrit la cible d'un signalement, ou sa nature si elle a disparu.</summary>
    /// <param name="type">Nature du contenu signalé.</param>
    /// <param name="id">Identifiant du contenu signalé.</param>
    /// <param name="cibles">Contenus résolus, par type puis par identifiant.</param>
    /// <returns>La cible affichée.</returns>
    private static CibleSignalee Cible(
        ReportTargetType type,
        Guid id,
        IReadOnlyDictionary<ReportTargetType, IReadOnlyDictionary<Guid, CibleSignalee>> cibles)
        => cibles.TryGetValue(type, out var parId) && parId.TryGetValue(id, out var cible)
            ? cible
            : new CibleSignalee(Libelle(type) + " supprimé", string.Empty, null);

    /// <summary>Intitulé d'un type de cible, pour nommer ce qui n'est plus là.</summary>
    /// <param name="type">Nature du contenu signalé.</param>
    /// <returns>Le nom de la cible.</returns>
    private static string Libelle(ReportTargetType type) => type switch
    {
        ReportTargetType.Article => "Article",
        ReportTargetType.Comment => "Commentaire",
        ReportTargetType.Resource => "Ressource",
        ReportTargetType.CommunityEvent => "Événement",
        ReportTargetType.Project => "Projet",
        _ => "Membre",
    };
}