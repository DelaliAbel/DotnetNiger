using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Inscriptions aux événements : ce que le membre connecté fait de la sienne, et ce que
/// l'administration voit pour un événement donné.
/// </summary>
/// <remarks>
/// La liste d'attente et la règle qui décide de qui l'occupe appartiennent au domaine. Cette
/// partie ne fait que constater ses décisions et en informer les personnes concernées.
/// </remarks>
public sealed partial class EventsController
{
    /// <summary>Inscrit le membre connecté à un événement.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le statut obtenu : confirmé ou en liste d'attente.</returns>
    /// <remarks>
    /// L'inscription et la lecture du compteur d'inscriptions se font dans une transaction :
    /// sans cela, deux requêtes simultanées pourraient dépasser la capacité.
    /// </remarks>
    /// <response code="201">Inscription enregistrée.</response>
    /// <response code="404">Événement introuvable.</response>
    /// <response code="409">Déjà inscrit, ou inscriptions closes.</response>
    [HttpPost("{id:guid}/registrations")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inscrire(Guid id, CancellationToken cancellationToken)
    {
        var membre = RequireUser(currentUser);

        // La transaction passe par la stratégie d'exécution : la reprise est active, et EF
        // refuse une transaction ouverte à la main. Le travail relu à chaque tentative —
        // l'événement, le compte des inscrits, l'inscription déjà existante — rend
        // l'opération idempotente, donc rejouable sans doublon.
        //
        // Le statut est déclaré dehors parce que la réponse HTTP le reprend : il doit
        // survivre à la transaction, qui peut être rejouée.
        var statut = EventRegistrationStatus.Confirmed;

        await unitOfWork.ExecuteInTransactionAsync(async jeton =>
        {
            var communityEvent = await unitOfWork.ReadRepository<CommunityEvent>().Query
                .FirstOrDefaultAsync(candidat => candidat.Id == id, jeton)
                ?? throw new DomainException("not_found", "Cet événement n'existe pas.");

            var maintenant = clock.GetUtcNow().UtcDateTime;
            var confirmes = await ConfirmedCountAsync(id, jeton);

            // La règle d'accueil et la liste d'attente sont décidées par le domaine, qui
            // connaît les dates de clôture et la capacité. L'API ne fait que la constater.
            if (!communityEvent.CanRegister(confirmes, maintenant))
            {
                throw new DomainException(
                    "registration_closed",
                    "Les inscriptions à cet événement sont closes.");
            }

            var dejaInscrit = await unitOfWork.ReadRepository<EventRegistration>().Query
                .AnyAsync(
                    inscription => inscription.CommunityEventId == id && inscription.MemberId == membre,
                    jeton);

            if (dejaInscrit)
            {
                throw new DomainException("already_exists", "Vous êtes déjà inscrit à cet événement.");
            }

            statut = communityEvent.IsFull(confirmes)
                ? EventRegistrationStatus.Waitlisted
                : EventRegistrationStatus.Confirmed;

            unitOfWork.Repository<EventRegistration>().Add(
                EventRegistration.Create(id, membre, maintenant, statut));

            await unitOfWork.SaveChangesAsync(jeton);

            // Le message part dans la transaction : si elle est annulée, le membre ne reçoit
            // pas la confirmation d'une inscription qui n'existe pas.
            await PrevenirInscriptionAsync(communityEvent, membre, statut, jeton);

            await unitOfWork.SaveChangesAsync(jeton);
        }, cancellationToken);

        return Created($"/api/events/{id}/registrations", new { eventId = id, status = statut });
    }

    /// <summary>Liste les inscriptions du membre connecté, pour son tableau de bord.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les inscriptions du membre, de la rencontre la plus proche à la plus lointaine.</returns>
    /// <response code="200">Inscriptions listées.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    [HttpGet("registrations/me")]
    [Authorize]
    [ProducesResponseType<IReadOnlyList<MyEventRegistrationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<MyEventRegistrationResponse>>> ListerMesInscriptions(
        CancellationToken cancellationToken)
    {
        var membre = RequireUser(currentUser);

        // Les rencontres annulées restent visibles : la personne s'était inscrite, et
        // une inscription qui disparaît sans un mot la laisserait sans savoir pourquoi.
        // La jointure passe par l'identifiant de l'événement : l'entité d'inscription ne
        // porte pas de navigation vers la rencontre, et en ajouter une pour les besoins
        // d'une seule requête n'en vaudrait pas le couplage.
        var inscriptions = await unitOfWork.ReadRepository<EventRegistration>().Query
            .AsNoTracking()
            .Where(inscription => inscription.MemberId == membre)
            .Join(
                unitOfWork.ReadRepository<CommunityEvent>().Query.AsNoTracking(),
                inscription => inscription.CommunityEventId,
                evenement => evenement.Id,
                (inscription, evenement) => new MyEventRegistrationResponse(
                    inscription.Id,
                    evenement.Id,
                    evenement.Slug,
                    evenement.Title,
                    evenement.StartsAtUtc,
                    inscription.Status))
            .OrderBy(inscription => inscription.Status)
            .ThenBy(inscription => inscription.StartsAtUtc)
            .ToListAsync(cancellationToken);

        return Ok(inscriptions);
    }

    /// <summary>Annule l'inscription du membre connecté.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse 204.</returns>
    /// <response code="204">Inscription annulée.</response>
    /// <response code="404">Aucune inscription à annuler.</response>
    [HttpDelete("{id:guid}/registrations")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnnulerSonInscription(Guid id, CancellationToken cancellationToken)
    {
        var membre = RequireUser(currentUser);

        var inscription = await unitOfWork.ReadRepository<EventRegistration>().Query
            .FirstOrDefaultAsync(
                candidat => candidat.CommunityEventId == id && candidat.MemberId == membre,
                cancellationToken);

        if (inscription is null)
        {
            return NotFound();
        }

        // Aucun message n'est envoyé ici, et c'est délibéré : le membre vient de demander
        // quelque chose, il n'a pas besoin d'un courriel pour apprendre qu'il l'a fait. Le
        // modèle « événement annulé » parle d'un événement qui n'aura pas lieu, ce n'est pas
        // la même chose qu'une personne qui se désiste.
        inscription.Cancel(clock.GetUtcNow().UtcDateTime);
        unitOfWork.Repository<EventRegistration>().Update(inscription);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Une place vient de se libérer : la personne la plus ancienne sur la liste
        // d'attente la prend. Sans cela, une place resterait perdue alors que quelqu'un
        // attend depuis plus longtemps.
        await PromouvoirLeSuivantAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>Liste les inscriptions à un événement, pour l'administration.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les inscriptions, de la plus récente à la plus ancienne.</returns>
    /// <response code="200">Inscriptions listées.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Événement introuvable.</response>
    [HttpGet("{id:guid}/registrations")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<IReadOnlyList<EventRegistrationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<EventRegistrationResponse>>> ListerLesInscriptions(
        Guid id,
        CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var evenementExiste = await unitOfWork.ReadRepository<CommunityEvent>().Query
            .AnyAsync(candidat => candidat.Id == id, cancellationToken);

        if (!evenementExiste)
        {
            return NotFound();
        }

        var inscriptions = await unitOfWork.ReadRepository<EventRegistration>().Query
            .Where(inscription => inscription.CommunityEventId == id)
            .OrderByDescending(inscription => inscription.RegisteredAtUtc)
            .ToListAsync(cancellationToken);

        // Les noms des membres sont résolus en une requête : la liste peut compter plusieurs
        // dizaines d'inscriptions, et une requête par ligne serait un aller-retour chacune.
        var noms = await NomsAsync(
            unitOfWork,
            [.. inscriptions.Select(inscription => inscription.MemberId).Distinct()],
            cancellationToken);

        return Ok(inscriptions
            .Select(inscription => ApiProjections.ToResponse(
                inscription,
                noms.GetValueOrDefault(inscription.MemberId, string.Empty)))
            .ToList());
    }
}