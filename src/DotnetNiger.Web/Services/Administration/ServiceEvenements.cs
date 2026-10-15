using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Un événement, tel que le liste l'administration.</summary>
/// <param name="Id">Identifiant de l'événement.</param>
/// <param name="Slug">Adresse de la fiche.</param>
/// <param name="Titre">Titre.</param>
/// <param name="Resume">Chapô.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="Type">Type d'événement, libellé du référentiel.</param>
/// <param name="Lieu">Lieu, ou <see langword="null"/> pour un événement purement en ligne.</param>
/// <param name="Statut">État de publication.</param>
/// <param name="Auteur">Nom du créateur.</param>
/// <param name="DebutUtc">Début de l'événement.</param>
/// <param name="FinUtc">Fin de l'événement.</param>
/// <param name="Confirmes">Inscriptions confirmées.</param>
/// <param name="Capacite">Nombre de places, si l'événement est limité.</param>
/// <param name="PublieLe">Date de publication, ou <see langword="null"/> si jamais publié.</param>
/// <param name="ModifieLe">Dernière modification, ou <see langword="null"/> si intact.</param>
public sealed record LigneEvenementAdmin(
    Guid Id,
    string Slug,
    string Titre,
    string Resume,
    CommunityEventFormat Format,
    string Type,
    string? Lieu,
    PublicationStatus Statut,
    string Auteur,
    DateTime DebutUtc,
    DateTime FinUtc,
    int Confirmes,
    int? Capacite,
    DateTime? PublieLe,
    DateTime? ModifieLe);

/// <summary>Liste complète lue pour la page Événements.</summary>
/// <param name="Evenements">Événements, du plus proche à venir au plus ancien.</param>
public sealed record EvenementsVue(IReadOnlyList<LigneEvenementAdmin> Evenements);

/// <summary>Événement complet, pour la reprise dans l'éditeur.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Titre">Titre.</param>
/// <param name="Resume">Chapô.</param>
/// <param name="Description">Description au format HTML.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="Type">Type d'événement.</param>
/// <param name="DebutUtc">Début.</param>
/// <param name="FinUtc">Fin.</param>
/// <param name="Lieu">Lieu, ou <see langword="null"/>.</param>
/// <param name="LienParticipation">Lien de participation, ou <see langword="null"/>.</param>
/// <param name="Capacite">Nombre de places, ou <see langword="null"/> si illimité.</param>
/// <param name="InscriptionsCloturentLe">Fin des inscriptions, ou <see langword="null"/>.</param>
/// <param name="ImageUrl">Image de couverture, ou <see langword="null"/>.</param>
public sealed record EvenementAdminDetail(
    Guid Id,
    string Titre,
    string Resume,
    string Description,
    CommunityEventFormat Format,
    string Type,
    DateTime DebutUtc,
    DateTime FinUtc,
    string? Lieu,
    string? LienParticipation,
    int? Capacite,
    DateTime? InscriptionsCloturentLe,
    string? ImageUrl);

/// <summary>
/// Lecture des événements pour l'administration : la liste et la reprise dans l'éditeur.
/// Les écritures passent par les points d'extrémité de l'API, comme pour le blog.
/// </summary>
/// <remarks>
/// Contrairement au blog, un événement n'a pas de rubrique : sa liste n'a donc rien à
/// charger en plus. Les types restent un référentiel de l'interface, pas des données —
/// ce sont trois valeurs de forme de réunion, pas des lignes en base.
/// </remarks>
public sealed class ServiceEvenements(IUnitOfWork uniteDeTravail, IMemberDirectory annuaire)
{
    private Task<EvenementsVue>? _lecture;

    /// <summary>Lit la liste des événements, une seule fois par changement.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les événements de l'administration.</returns>
    public Task<EvenementsVue> LireAsync(CancellationToken cancellationToken = default)
        => (_lecture ??= LireInterneAsync(cancellationToken));

    /// <summary>Lit un événement en entier, pour l'ouvrir dans l'éditeur.</summary>
    /// <param name="evenementId">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement, ou <see langword="null"/> s'il est introuvable.</returns>
    public async Task<EvenementAdminDetail?> LireDetailAsync(
        Guid evenementId,
        CancellationToken cancellationToken = default)
    {
        var evenement = await uniteDeTravail.ReadRepository<CommunityEvent>().Query
            .AsNoTracking()
            .FirstOrDefaultAsync(candidat => candidat.Id == evenementId, cancellationToken);

        return evenement is null
            ? null
            : new EvenementAdminDetail(
                evenement.Id,
                evenement.Title,
                evenement.Summary,
                evenement.Description,
                evenement.Format,
                evenement.Kind,
                evenement.StartsAtUtc,
                evenement.EndsAtUtc,
                evenement.Location,
                evenement.OnlineUrl,
                evenement.Capacity,
                evenement.RegistrationClosesAtUtc,
                evenement.CoverImageUrl);
    }

    /// <summary>Relit la liste à la prochaine demande, après une écriture.</summary>
    public void Oublie() => _lecture = null;

    private async Task<EvenementsVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        // Les événements brouillons et archivés sont exclus : la liste d'administration
        // montre ce qui est ou sera publié, pas l'historique de ce qui ne le sera plus.
        var evenements = await uniteDeTravail.ReadRepository<CommunityEvent>().Query
            .AsNoTracking()
            .Where(evenement => evenement.Status != PublicationStatus.Archived)
            .OrderBy(evenement => evenement.StartsAtUtc)
            .ToListAsync(cancellationToken);

        var auteurs = await annuaire.ObtenirPlusieursAsync(
            [.. evenements.Select(candidat => candidat.CreatedById).Distinct()],
            cancellationToken);

        var confirmes = await CompterInscriptionsAsync(
            [.. evenements.Select(candidat => candidat.Id)],
            cancellationToken);

        return new EvenementsVue(
            [.. evenements.Select(evenement => new LigneEvenementAdmin(
                evenement.Id,
                evenement.Slug,
                evenement.Title,
                evenement.Summary,
                evenement.Format,
                evenement.Kind,
                evenement.Location,
                evenement.Status,
                auteurs.GetValueOrDefault(evenement.CreatedById)?.FullName ?? string.Empty,
                evenement.StartsAtUtc,
                evenement.EndsAtUtc,
                confirmes.GetValueOrDefault(evenement.Id),
                evenement.Capacity,
                evenement.PublishedAtUtc,
                evenement.UpdatedAtUtc))]);
    }

    /// <summary>
    /// Compte les inscriptions confirmées, événement par événement.
    /// </summary>
    /// <remarks>
    /// Une seule requête pour tous les événements : les compter un par un en ferait
    /// autant que la liste est longue, pour un chiffre affiché sur une seule colonne.
    /// </remarks>
    /// <param name="evenementIds">Identifiants à dénombrer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Nombre d'inscriptions confirmées, par événement.</returns>
    private async Task<Dictionary<Guid, int>> CompterInscriptionsAsync(
        IReadOnlyCollection<Guid> evenementIds,
        CancellationToken cancellationToken)
    {
        if (evenementIds.Count is 0)
        {
            return [];
        }

        var lignes = await uniteDeTravail.ReadRepository<EventRegistration>().Query
            .AsNoTracking()
            .Where(inscription => evenementIds.Contains(inscription.CommunityEventId)
                && inscription.Status == EventRegistrationStatus.Confirmed)
            .GroupBy(inscription => inscription.CommunityEventId)
            .Select(groupe => new { EvenementId = groupe.Key, Nombre = groupe.Count() })
            .ToListAsync(cancellationToken);

        return lignes.ToDictionary(ligne => ligne.EvenementId, ligne => ligne.Nombre);
    }
}