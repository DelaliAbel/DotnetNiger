using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Membres;

/// <summary>Une rencontre à laquelle le membre est inscrit.</summary>
/// <param name="InscriptionId">Identifiant de l'inscription, pour l'annuler.</param>
/// <param name="EvenementId">Identifiant de l'événement.</param>
/// <param name="Slug">Adresse de la fiche.</param>
/// <param name="Titre">Titre de la rencontre.</param>
/// <param name="DebutUtc">Début.</param>
/// <param name="Statut">État de l'inscription.</param>
public sealed record InscriptionVue(
    Guid InscriptionId,
    Guid EvenementId,
    string Slug,
    string Titre,
    DateTime DebutUtc,
    EventRegistrationStatus Statut);

/// <summary>Une inscription du membre connecté, pour son tableau de bord.</summary>
/// <param name="Inscriptions">Rencontres réservées, de la plus proche à la plus lointaine.</param>
public sealed record InscriptionsVue(IReadOnlyList<InscriptionVue> Inscriptions);

/// <summary>
/// Inscriptions du membre aux événements.
/// </summary>
/// <remarks>
/// Le service ne fait qu'une lecture : décider si l'inscription est encore ouverte, si
/// une place s'est libérée ou si la rencontre est passée appartient au domaine et aux
/// endpoints qui les exposent déjà.
/// </remarks>
public sealed class ServiceInscriptionsMembre(IUnitOfWork uniteDeTravail)
{
    /// <summary>Les inscriptions du membre, rencontres passées comprise.</summary>
    /// <param name="membreId">Identifiant du membre connecté.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les rencontres réservées, triées par date croissante.</returns>
    public async Task<InscriptionsVue> ListerAsync(Guid membreId, CancellationToken cancellationToken = default)
    {
        // Le tri porte sur l'entité jointe, pas sur <see cref="InscriptionVue"/> : une fois
        // projeté dans un record, le membre de la projection n'est plus une colonne et
        // SQL Server n'a rien à lui opposer. Trier avant de projeter laisse le
        // générateur traduire l'ordre sur Status et StartsAtUtc, qui sont des colonnes.
        var inscriptions = await uniteDeTravail.ReadRepository<EventRegistration>().Query
            .AsNoTracking()
            .Where(inscription => inscription.MemberId == membreId)
            .Join(
                uniteDeTravail.ReadRepository<CommunityEvent>().Query.AsNoTracking(),
                inscription => inscription.CommunityEventId,
                evenement => evenement.Id,
                (inscription, evenement) => new { inscription, evenement })
            // Confirmée d'abord, annulée ensuite : une place libérée attend d'être
            // remise, et doit se voir avant les rencontres que le membre a quittées.
            .OrderBy(pair => pair.inscription.Status)
            .ThenBy(pair => pair.evenement.StartsAtUtc)
            .Select(pair => new InscriptionVue(
                pair.inscription.Id,
                pair.evenement.Id,
                pair.evenement.Slug,
                pair.evenement.Title,
                pair.evenement.StartsAtUtc,
                pair.inscription.Status))
            .ToListAsync(cancellationToken);

        return new InscriptionsVue(inscriptions);
    }
}
