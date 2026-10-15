using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Inscriptions;

/// <summary>Issue d'une demande d'inscription à un événement.</summary>
/// <param name="Reussi">Vrai si la place est réservée.</param>
/// <param name="Statut">Statut obtenu, une fois l'inscription écrite.</param>
/// <param name="Message">Motif du refus, si la demande n'a pas abouti.</param>
public readonly record struct ResultatInscription(bool Reussi, EventRegistrationStatus Statut, string Message)
{
    /// <summary>Inscription accordée, place confirmée ou mise en liste d'attente.</summary>
    /// <param name="statut">Statut obtenu.</param>
    /// <returns>Une inscription réussie.</returns>
    public static ResultatInscription Accordee(EventRegistrationStatus statut)
        => new(true, statut, string.Empty);

    /// <summary>Demande refusée, avec le motif à afficher au membre.</summary>
    /// <param name="message">Motif du refus.</param>
    /// <returns>Une inscription refusée.</returns>
    public static ResultatInscription Refusee(string message)
        => new(false, default, message);
}

/// <summary>
/// Inscription d'un membre à un événement, appelée aussi bien par l'API que par le
/// formulaire de la fiche événement.
/// </summary>
/// <remarks>
/// La règle d'accueil — date de clôture, capacité, liste d'attente — appartient au
/// domaine, qui connaît les conditions réelles de la manifestation. Ce service se
/// contente de la constater et d'écrire l'inscription, sans jamais la redéfinir : deux
/// points d'entrée qui décident différemment donneraient au membre deux réponses
/// contradictoires selon le chemin emprunté.
/// </remarks>
public sealed class ServiceInscriptionEvenement(
    IUnitOfWork uniteDeTravail,
    TimeProvider horloge,
    ILogger<ServiceInscriptionEvenement> logger)
{
    /// <summary>Réserve une place pour le membre, ou explique le refus.</summary>
    /// <param name="evenementId">Identifiant de l'événement.</param>
    /// <param name="membreId">Identifiant du membre connecté.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le résultat de la demande, motif compris en cas de refus.</returns>
    public async Task<ResultatInscription> InscrireAsync(
        Guid evenementId,
        Guid membreId,
        CancellationToken cancellationToken = default)
    {
        var evenement = await uniteDeTravail.ReadRepository<CommunityEvent>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == evenementId, cancellationToken);

        if (evenement is null)
        {
            return ResultatInscription.Refusee("Cet événement n'existe pas.");
        }

        var maintenant = horloge.GetUtcNow().UtcDateTime;
        var confirmes = await ConfirmerAsync(evenementId, cancellationToken);

        if (!evenement.CanRegister(confirmes, maintenant))
        {
            return ResultatInscription.Refusee("Les inscriptions à cet événement sont closes.");
        }

        var dejaInscrit = await uniteDeTravail.ReadRepository<EventRegistration>().Query
            .AnyAsync(
                inscription => inscription.CommunityEventId == evenementId && inscription.MemberId == membreId,
                cancellationToken);

        if (dejaInscrit)
        {
            return ResultatInscription.Refusee("Vous êtes déjà inscrit à cet événement.");
        }

        // Un événement complet refuse l'inscription : le domaine l'a déjà décidé dans
        // CanRegister, qui tient compte de la capacité. Aucune liste d'attente n'existe,
        // il n'y a donc pas de second statut à décider ici.
        uniteDeTravail.Repository<EventRegistration>().Add(
            EventRegistration.Create(evenementId, membreId, maintenant, EventRegistrationStatus.Confirmed));

        await uniteDeTravail.SaveChangesAsync(cancellationToken);

        InscriptionLog.InscriptionEnregistree(
            logger,
            membreId,
            evenementId,
            evenement.Slug);

        return ResultatInscription.Accordee(EventRegistrationStatus.Confirmed);
    }

    /// <summary>Libère la place du membre sur l'événement.</summary>
    /// <param name="evenementId">Identifiant de l'événement.</param>
    /// <param name="membreId">Identifiant du membre connecté.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le résultat de la demande, motif compris en cas de refus.</returns>
    public async Task<ResultatInscription> AnnulerAsync(
        Guid evenementId,
        Guid membreId,
        CancellationToken cancellationToken = default)
    {
        var inscription = await uniteDeTravail.ReadRepository<EventRegistration>().Query
            .FirstOrDefaultAsync(
                candidat => candidat.CommunityEventId == evenementId && candidat.MemberId == membreId,
                cancellationToken);

        if (inscription is null)
        {
            return ResultatInscription.Refusee("Vous n'êtes pas inscrit à cet événement.");
        }

        // Aucun message n'est envoyé ici : le membre vient de demander quelque chose, il
        // n'a pas besoin d'un courriel pour apprendre qu'il l'a fait.
        inscription.Cancel(horloge.GetUtcNow().UtcDateTime);
        uniteDeTravail.Repository<EventRegistration>().Update(inscription);
        await uniteDeTravail.SaveChangesAsync(cancellationToken);

        return ResultatInscription.Accordee(EventRegistrationStatus.Cancelled);
    }

    /// <summary>Indique si le membre occupe déjà une place, confirmée ou en attente.</summary>
    /// <param name="evenementId">Identifiant de l'événement.</param>
    /// <param name="membreId">Identifiant du membre.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns><see langword="true"/> si une inscription existe déjà.</returns>
    public Task<bool> EstInscritAsync(
        Guid evenementId,
        Guid membreId,
        CancellationToken cancellationToken = default)
        => uniteDeTravail.ReadRepository<EventRegistration>().Query
            .AnyAsync(
                inscription => inscription.CommunityEventId == evenementId && inscription.MemberId == membreId,
                cancellationToken);

    private Task<int> ConfirmerAsync(Guid evenementId, CancellationToken cancellationToken)
        => uniteDeTravail.ReadRepository<EventRegistration>().Query
            .CountAsync(
                inscription => inscription.CommunityEventId == evenementId
                    && inscription.Status == EventRegistrationStatus.Confirmed,
                cancellationToken);
}
