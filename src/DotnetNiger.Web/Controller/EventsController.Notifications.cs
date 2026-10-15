using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Ce que les événements disent aux personnes concernées.
/// </summary>
/// <remarks>
/// Une inscription, une promotion depuis la liste d'attente, un changement de date et une
/// annulation ne se vivent pas de la même façon : chacune a donc son message. Tous ces
/// messages partent dans la transaction de l'action qui les déclenche, pour qu'une
/// annulation d'écriture ne laisse pas un membre croire à une inscription qui n'existe pas.
/// </remarks>
public sealed partial class EventsController
{
    /// <summary>
    /// Prévient le membre du résultat de son inscription.
    /// </summary>
    /// <param name="communityEvent">Événement visé.</param>
    /// <param name="membreId">Identifiant du membre inscrit.</param>
    /// <param name="statut">Statut obtenu.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche d'achèvement.</returns>
    /// <remarks>
    /// Une place et une liste d'attente ne se vivent pas de la même façon : on ne peut pas
    /// dire à quelqu'un qu'il a une place, puis lui écrire qu'il n'y en a plus. Le message
    /// dépend donc du statut réellement obtenu.
    /// </remarks>
    private async Task PrevenirInscriptionAsync(
        CommunityEvent communityEvent,
        Guid membreId,
        EventRegistrationStatus statut,
        CancellationToken cancellationToken)
    {
        var url = emailOptions.Value.BuildUrl($"/evenements/{communityEvent.Slug}");

        if (statut == EventRegistrationStatus.Waitlisted)
        {
            await emails.DispatchAsync(
                EmailTemplateKeys.EventRegistrationWaitlisted,
                membreId,
                EmailValues.De(
                    (EmailVariables.TitreEvenement, communityEvent.Title),
                    (EmailVariables.UrlEvenement, url),
                    (EmailVariables.UrlDesinscriptionEvenement, $"{url}/inscriptions")),
                cancellationToken);

            return;
        }

        // Seul le message de confirmation s'adresse le membre par son nom : il s'agit de
        // lui dire bienvenue, là où les autres ne sont que des avis de gestion.
        var contact = await members.ObtenirAsync(membreId, cancellationToken);

        await emails.DispatchAsync(
            EmailTemplateKeys.EventRegistrationConfirmed,
            membreId,
            EmailValues.De(
                (EmailVariables.NomMembre, contact?.FullName ?? string.Empty),
                (EmailVariables.TitreEvenement, communityEvent.Title),
                (EmailVariables.DateEvenement, EmailFormat.Date(communityEvent.StartsAtUtc)),
                (EmailVariables.LieuEvenement, LieuLisible(communityEvent)),
                (EmailVariables.UrlEvenement, url)),
            cancellationToken);
    }

    /// <summary>
    /// Attend la place que vient de libérer un désistement.
    /// </summary>
    /// <param name="eventId">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns><see langword="true"/> si quelqu'un a été promu.</returns>
    /// <remarks>
    /// L'ordre est celui de l'inscription : c'est la personne qui attend depuis le plus
    /// longtemps qui reçoit la place. La promotion n'a lieu que si l'événement a encore de
    /// la place, et sa demande est retirée de la liste d'attente.
    /// </remarks>
    private async Task<bool> PromouvoirLeSuivantAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        var communityEvent = await ChargerEvenement(eventId, cancellationToken);

        if (communityEvent is null)
        {
            return false;
        }

        var confirmes = await ConfirmedCountAsync(eventId, cancellationToken);

        if (communityEvent.IsFull(confirmes))
        {
            return false;
        }

        var suivant = await unitOfWork.ReadRepository<EventRegistration>().Query
            .Where(inscription => inscription.CommunityEventId == eventId
                && inscription.Status == EventRegistrationStatus.Waitlisted)
            .OrderBy(inscription => inscription.RegisteredAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (suivant is null)
        {
            return false;
        }

        suivant.ConfirmFromWaitlist();
        unitOfWork.Repository<EventRegistration>().Update(suivant);

        await emails.DispatchAsync(
            EmailTemplateKeys.EventWaitlistPromoted,
            suivant.MemberId,
            EmailValues.De(
                (EmailVariables.TitreEvenement, communityEvent.Title),
                (EmailVariables.UrlEvenement, emailOptions.Value.BuildUrl($"/evenements/{communityEvent.Slug}"))),
            cancellationToken);

        PrevenirPromotion(logger, communityEvent.Title, suivant.MemberId);

        return true;
    }

    /// <summary>
    /// Décrit le lieu d'un événement de façon lisible.
    /// </summary>
    /// <param name="communityEvent">Événement visé.</param>
    /// <returns>Le lieu saisi, ou une mention de l'événement en ligne.</returns>
    private static string LieuLisible(CommunityEvent communityEvent) =>
        string.IsNullOrWhiteSpace(communityEvent.Location)
            ? "En ligne"
            : communityEvent.Location;

    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Information,
        Message = "Une place de {Evenement} a été attribuée au membre {Membre} depuis la liste d'attente")]
    private static partial void PrevenirPromotion(ILogger logger, string evenement, Guid membre);

    /// <summary>
    /// Prévient les inscrits qu'un événement qu'ils suivent a changé.
    /// </summary>
    /// <param name="communityEvent">Événement modifié.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche d'achèvement.</returns>
    private async Task PrevenirModificationAsync(
        CommunityEvent communityEvent,
        CancellationToken cancellationToken)
    {
        var destinataires = await InscritsAsync(communityEvent.Id, cancellationToken);
        var valeurs = EmailValues.De(
            (EmailVariables.TitreEvenement, communityEvent.Title),
            (EmailVariables.DateEvenement, EmailFormat.Date(communityEvent.StartsAtUtc)),
            (EmailVariables.LieuEvenement, LieuLisible(communityEvent)),
            (EmailVariables.UrlEvenement, emailOptions.Value.BuildUrl($"/evenements/{communityEvent.Slug}")));

        foreach (var membreId in destinataires)
        {
            await emails.DispatchAsync(
                EmailTemplateKeys.EventUpdated,
                membreId,
                valeurs,
                cancellationToken);
        }

        PrevenirModification(logger, communityEvent.Title, destinataires.Count);
    }

    /// <summary>
    /// Prévient les inscrits qu'un événement n'aura pas lieu.
    /// </summary>
    /// <param name="communityEvent">Événement retiré.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche d'achèvement.</returns>
    private async Task PrevenirAnnulationAsync(
        CommunityEvent communityEvent,
        CancellationToken cancellationToken)
    {
        var destinataires = await InscritsAsync(communityEvent.Id, cancellationToken);

        // Le modèle annonce la date à laquelle l'événement était prévu, pas celle de son
        // retrait : la seconde ne lui dirait rien et serait même fausse.
        var valeurs = EmailValues.De(
            (EmailVariables.TitreEvenement, communityEvent.Title),
            (EmailVariables.DateEvenement, EmailFormat.Date(communityEvent.StartsAtUtc)));

        foreach (var membreId in destinataires)
        {
            await emails.DispatchAsync(
                EmailTemplateKeys.EventCancelled,
                membreId,
                valeurs,
                cancellationToken);
        }

        PrevenirAnnulation(logger, communityEvent.Title, destinataires.Count);
    }

    /// <summary>
    /// Rend les membres inscrits à un événement, places confirmées comme liste d'attente.
    /// </summary>
    /// <param name="eventId">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les identifiants des inscrits.</returns>
    /// <remarks>
    /// Les personnes en liste d'attente sont comprises : elles ont réservé du temps pour
    /// venir, et une annulation ou un changement de date les concerne autant que les autres.
    /// </remarks>
    private async Task<IReadOnlyList<Guid>> InscritsAsync(Guid eventId, CancellationToken cancellationToken) =>
        await unitOfWork.ReadRepository<EventRegistration>().Query
            .Where(inscription => inscription.CommunityEventId == eventId
                && inscription.Status != EventRegistrationStatus.Cancelled)
            .Select(inscription => inscription.MemberId)
            .ToListAsync(cancellationToken);

    [LoggerMessage(
        EventId = 101,
        Level = LogLevel.Information,
        Message = "Modification de {Evenement} annoncée à {Nombre} inscrit(s)")]
    private static partial void PrevenirModification(ILogger logger, string evenement, int nombre);

    [LoggerMessage(
        EventId = 102,
        Level = LogLevel.Information,
        Message = "Annulation de {Evenement} annoncée à {Nombre} inscrit(s)")]
    private static partial void PrevenirAnnulation(ILogger logger, string evenement, int nombre);
}