using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Un abonnement à la lettre d'information.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Courriel">Adresse abonnée.</param>
/// <param name="Statut">État de l'abonnement.</param>
/// <param name="DemandeLe">Date de la demande (UTC).</param>
/// <param name="ConfirmeLe">Date de confirmation (UTC), ou <see langword="null"/>.</param>
/// <param name="DesabonneLe">Date de désabonnement (UTC), ou <see langword="null"/>.</param>
public sealed record LigneAbonneAdmin(
    Guid Id,
    string Courriel,
    NewsletterStatus Statut,
    DateTime DemandeLe,
    DateTime? ConfirmeLe,
    DateTime? DesabonneLe);

/// <summary>Tous les abonnements, lus pour la page Abonnés.</summary>
/// <param name="Abonnes">Abonnements, du plus récent au plus ancien.</param>
public sealed record AbonneesVue(IReadOnlyList<LigneAbonneAdmin> Abonnes);

/// <summary>
/// Lecture des abonnements pour l'administration.
/// </summary>
/// <remarks>
/// Cette page ne comporte aucune écriture, et c'est délibéré : l'API n'expose aucun moyen de
/// désabonner un membre à la place d'un autre. Un désabonnement se fait toujours depuis la
/// lettre elle-même, avec son lien — c'est ce qui garantit qu'un abonné retiré l'a bien
/// demandé lui-même, et non qu'un tiers l'a décidé à sa place.
/// </remarks>
public sealed class ServiceAbonnees(IUnitOfWork uniteDeTravail)
{
    private Task<AbonneesVue>? _lecture;

    /// <summary>Lit les abonnements, une seule fois par changement.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les abonnements à la lettre.</returns>
    public Task<AbonneesVue> LireAsync(CancellationToken cancellationToken = default)
        => (_lecture ??= LireInterneAsync(cancellationToken));

    /// <summary>Relit la liste à la prochaine demande.</summary>
    public void Oublie() => _lecture = null;

    private async Task<AbonneesVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        var abonnements = await uniteDeTravail.ReadRepository<NewsletterSubscription>().Query
            .AsNoTracking()
            .OrderByDescending(abonnement => abonnement.RequestedAtUtc)
            .ToListAsync(cancellationToken);

        return new AbonneesVue(
            [.. abonnements.Select(abonnement => new LigneAbonneAdmin(
                abonnement.Id,
                abonnement.Email,
                abonnement.Status,
                abonnement.RequestedAtUtc,
                abonnement.ConfirmedAtUtc,
                abonnement.UnsubscribedAtUtc))]);
    }
}
