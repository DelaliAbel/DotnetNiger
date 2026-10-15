using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Queries;

/// <summary>Événement publié et publié, tel que le sert l'API et le lit le site public.</summary>
/// <param name="Id">Identifiant de l'événement.</param>
/// <param name="Slug">Adresse de la fiche.</param>
/// <param name="Titre">Titre.</param>
/// <param name="Type">Type d'événement (meetup, conférence, atelier…).</param>
/// <param name="Lieu">Lieu, ou mention de la modalité en ligne.</param>
/// <param name="Format">Modalité réelle : en personne, en ligne ou hybride.</param>
/// <param name="LienParticipation">Adresse de participation en ligne.</param>
/// <param name="Resume">Résumé.</param>
/// <param name="Description">Description complète.</param>
/// <param name="ImageUrl">Image de couverture.</param>
/// <param name="DebutUtc">Début.</param>
/// <param name="FinUtc">Fin.</param>
/// <param name="Capacite">Nombre de places, si limité.</param>
/// <param name="Confirmes">Inscriptions confirmées.</param>
public sealed record EvenementLu(
    Guid Id,
    string Slug,
    string Titre,
    string Type,
    string Lieu,
    CommunityEventFormat Format,
    string? LienParticipation,
    string Resume,
    string Description,
    string? ImageUrl,
    DateTime DebutUtc,
    DateTime FinUtc,
    int? Capacite,
    int Confirmes);

/// <summary>
/// Lecture des événements publiés.
/// </summary>
/// <remarks>
/// Partagé par l'API et par les pages de l'agenda, pour la même raison que les articles :
/// l'agenda du site et la liste renvoyée par l'API doivent énumérer les mêmes
/// rendez-vous, dans le même ordre, et ceci quel que soit le tri demandé.
/// </remarks>
public sealed class EvenementQueries(IUnitOfWork uniteDeTravail)
{
    /// <summary>Événements publiés, filtrés et triés.</summary>
    /// <param name="aVenir">Ne garder que les rendez-vous non terminés.</param>
    /// <param name="format">Ne garder que cette modalité.</param>
    /// <param name="recherche">Texte cherché dans le titre, le résumé et le lieu.</param>
    /// <param name="trier">Ordre demandé ; par défaut, le rendez-vous le plus proche.</param>
    /// <param name="page">Page demandée, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les événements de la page demandée, et le nombre total correspondant.</returns>
    public async Task<(IReadOnlyList<EvenementLu> Evenements, int Total)> ListerAsync(
        bool? aVenir,
        CommunityEventFormat? format,
        string? recherche,
        string? trier,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var requete = uniteDeTravail.ReadRepository<CommunityEvent>().Query
            .Where(communityEvent => communityEvent.Status == PublicationStatus.Published);

        if (aVenir is { } wanted)
        {
            requete = wanted
                ? requete.Where(communityEvent => communityEvent.EndsAtUtc >= DateTime.UtcNow)
                : requete.Where(communityEvent => communityEvent.EndsAtUtc < DateTime.UtcNow);
        }

        if (format is { } modalite)
        {
            requete = requete.Where(communityEvent => communityEvent.Format == modalite);
        }

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var terme = recherche.Trim();
            requete = requete.Where(communityEvent =>
                communityEvent.Title.Contains(terme)
                || communityEvent.Summary.Contains(terme)
                || (communityEvent.Location != null && communityEvent.Location.Contains(terme)));
        }

        requete = trier switch
        {
            "date-desc" => requete.OrderByDescending(communityEvent => communityEvent.StartsAtUtc),
            "recent" => requete
                .OrderByDescending(communityEvent => communityEvent.PublishedAtUtc)
                .ThenByDescending(communityEvent => communityEvent.StartsAtUtc),
            "ancien" => requete
                .OrderBy(communityEvent => communityEvent.PublishedAtUtc)
                .ThenBy(communityEvent => communityEvent.StartsAtUtc),
            "titre" => requete.OrderBy(communityEvent => communityEvent.Title),
            "titre-desc" => requete.OrderByDescending(communityEvent => communityEvent.Title),
            _ => requete.OrderBy(communityEvent => communityEvent.StartsAtUtc),
        };

        var total = await requete.CountAsync(cancellationToken);

        var evenements = await requete
            .Skip((Math.Max(page, 1) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var confirmes = await ConfirmesAsync(
            [.. evenements.Select(evenement => evenement.Id)],
            cancellationToken);

        return ([.. evenements.Select(evenement => Projeter(evenement, confirmes.GetValueOrDefault(evenement.Id, 0)))], total);
    }

    /// <summary>Un événement publié, par son slug.</summary>
    /// <param name="slug">Slug de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement, ou <see langword="null"/> s'il n'existe pas ou n'est pas publié.</returns>
    public async Task<EvenementLu?> LireAsync(string slug, CancellationToken cancellationToken = default)
    {
        var evenement = await uniteDeTravail.ReadRepository<CommunityEvent>().Query
            .FirstOrDefaultAsync(
                candidat => candidat.Slug == slug && candidat.Status == PublicationStatus.Published,
                cancellationToken);

        if (evenement is null)
        {
            return null;
        }

        var confirmes = await ConfirmesAsync([evenement.Id], cancellationToken);
        return Projeter(evenement, confirmes.GetValueOrDefault(evenement.Id, 0));
    }

    /// <summary>
    /// Nombre d'inscriptions confirmées par événement. Un seul groupement évite une
    /// requête par rendez-vous de la page.
    /// </summary>
    private async Task<Dictionary<Guid, int>> ConfirmesAsync(
        IReadOnlyCollection<Guid> evenementIds,
        CancellationToken cancellationToken)
    {
        if (evenementIds.Count == 0)
        {
            return [];
        }

        return await uniteDeTravail.ReadRepository<EventRegistration>().Query
            .Where(inscription => evenementIds.Contains(inscription.CommunityEventId)
                && inscription.Status == EventRegistrationStatus.Confirmed)
            .GroupBy(inscription => inscription.CommunityEventId)
            .Select(groupe => new { Id = groupe.Key, Total = groupe.Count() })
            .ToDictionaryAsync(resultat => resultat.Id, resultat => resultat.Total, cancellationToken);
    }

    /// <summary>
    /// Compose le lieu affiché. Un rendez-vous en ligne n'a pas d'adresse : il se
    /// désigne par sa modalité, que le lecteur doit comprendre sans la traduire.
    /// </summary>
    private static EvenementLu Projeter(CommunityEvent evenement, int confirmes)
    {
        var enLigne = evenement.Format is CommunityEventFormat.Online or CommunityEventFormat.Hybrid;

        return new EvenementLu(
            evenement.Id,
            evenement.Slug,
            evenement.Title,
            evenement.Kind,
            enLigne ? "En ligne" : evenement.Location ?? "Lieu à confirmer",
            evenement.Format,
            evenement.OnlineUrl,
            evenement.Summary,
            evenement.Description,
            evenement.CoverImageUrl,
            evenement.StartsAtUtc,
            evenement.EndsAtUtc,
            evenement.Capacity,
            confirmes);
    }
}
