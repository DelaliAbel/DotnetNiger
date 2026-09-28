using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Événements de la communauté et inscriptions.
/// </summary>
[Route("api/events")]
[Tags("Événements")]
public sealed partial class EventsController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IEmailDispatcher emails,
    IMemberDirectory members,
    INewsletterDiffusion newsletter,
    IOptions<EmailOptions> emailOptions,
    TimeProvider clock,
    ILogger<EventsController> logger) : ApiControllerBase
{
    /// <summary>Liste les événements publiés, à venir par défaut.</summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="aVenir">Ne garder que les événements non terminés.</param>
    /// <param name="format">Filtre facultatif par format.</param>
    /// <param name="recherche">Filtre facultatif sur le titre ou le résumé.</param>
    /// <param name="trier">Ordre facultatif : <c>date</c>, <c>date-desc</c>, <c>recent</c>, <c>ancien</c>, <c>titre</c> ou <c>titre-desc</c>.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés d'événements.</returns>
    /// <response code="200">Page d'événements.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PageResponse<EventSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResponse<EventSummaryResponse>>> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool aVenir = true,
        [FromQuery] CommunityEventFormat? format = null,
        [FromQuery] string? recherche = null,
        [FromQuery] string? trier = null,
        CancellationToken cancellationToken = default)
    {
        var requete = unitOfWork.ReadRepository<CommunityEvent>().Query
            .Where(communityEvent => communityEvent.Status == PublicationStatus.Published);

        if (aVenir)
        {
            requete = requete.Where(communityEvent => communityEvent.EndsAtUtc >= DateTime.UtcNow);
        }

        if (format is { } valeur)
        {
            requete = requete.Where(communityEvent => communityEvent.Format == valeur);
        }

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var terme = recherche.Trim();
            requete = requete.Where(communityEvent =>
                communityEvent.Title.Contains(terme) || communityEvent.Summary.Contains(terme));
        }

        requete = trier switch
        {
            "date-desc" => requete.OrderByDescending(communityEvent => communityEvent.StartsAtUtc),
            "recent" => requete
                .OrderByDescending(communityEvent => communityEvent.PublishedAtUtc)
                .ThenBy(communityEvent => communityEvent.Title),
            "ancien" => requete
                .OrderBy(communityEvent => communityEvent.PublishedAtUtc)
                .ThenBy(communityEvent => communityEvent.Title),
            "titre" => requete.OrderBy(communityEvent => communityEvent.Title),
            "titre-desc" => requete.OrderByDescending(communityEvent => communityEvent.Title),
            _ => requete.OrderBy(communityEvent => communityEvent.StartsAtUtc),
        };

        var demande = new PageRequest(page, pageSize);
        var total = await requete.CountAsync(cancellationToken);

        if (total == 0)
        {
            return ApiProjections.ToPage(
                PagedResultFactory.Empty<CommunityEvent>(demande),
                communityEvent => ApiProjections.ToSummary(communityEvent, 0));
        }

        var items = await requete
            .Skip(demande.Skip)
            .Take(demande.PageSize)
            .ToListAsync(cancellationToken);

        // Un comptage groupé par événement évite une requête par événement de la page.
        var confirmes = await unitOfWork.ReadRepository<EventRegistration>().Query
            .Where(inscription => inscription.Status == EventRegistrationStatus.Confirmed)
            .GroupBy(inscription => inscription.CommunityEventId)
            .Select(groupe => new { Id = groupe.Key, Total = groupe.Count() })
            .ToDictionaryAsync(resultat => resultat.Id, resultat => resultat.Total, cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<CommunityEvent>(items, demande.PageNumber, demande.PageSize, total),
            communityEvent => ApiProjections.ToSummary(
                communityEvent,
                confirmes.GetValueOrDefault(communityEvent.Id)));
    }

    /// <summary>Lit un événement publié.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement et son nombre d'inscriptions.</returns>
    /// <response code="200">Événement trouvé.</response>
    /// <response code="404">Événement introuvable ou non publié.</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<EventSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventSummaryResponse>> Lire(
        Guid id,
        CancellationToken cancellationToken)
    {
        var communityEvent = await unitOfWork.ReadRepository<CommunityEvent>().Query
            .FirstOrDefaultAsync(
                candidat => candidat.Id == id && candidat.Status == PublicationStatus.Published,
                cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        return Ok(ApiProjections.ToSummary(
            communityEvent,
            await ConfirmedCountAsync(id, cancellationToken)));
    }

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

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var communityEvent = await unitOfWork.ReadRepository<CommunityEvent>().Query
                .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken)
                ?? throw new DomainException("not_found", "Cet événement n'existe pas.");

            var maintenant = clock.GetUtcNow().UtcDateTime;
            var confirmes = await ConfirmedCountAsync(id, cancellationToken);

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
                    cancellationToken);

            if (dejaInscrit)
            {
                throw new DomainException("already_exists", "Vous êtes déjà inscrit à cet événement.");
            }

            var statut = communityEvent.IsFull(confirmes)
                ? EventRegistrationStatus.Waitlisted
                : EventRegistrationStatus.Confirmed;

            unitOfWork.Repository<EventRegistration>().Add(
                EventRegistration.Create(id, membre, maintenant, statut));

            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Le message part dans la transaction : si elle est annulée, le membre ne reçoit
            // pas la confirmation d'une inscription qui n'existe pas.
            await PrevenirInscriptionAsync(communityEvent, membre, statut, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Created($"/api/events/{id}/registrations", new { eventId = id, status = statut });
        }
        catch
        {
            // Sans annulation, l'écriture en attente resterait visible pour la requête suivante.
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
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

    /// <summary>Crée un événement, en brouillon ou publié.</summary>
    /// <param name="request">Contenu de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement créé.</returns>
    /// <remarks>
    /// Un événement est une initiative de l'association : sa création est réservée à
    /// l'administration, contrairement aux ressources et aux articles, que tout membre
    /// peut proposer.
    /// </remarks>
    /// <response code="201">Événement créé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<EventResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventResponse>> Creer(
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        var auteur = RequireAdministrateur(currentUser);

        var communityEvent = new CommunityEvent(
            auteur,
            request.Title,
            request.Summary,
            request.Description,
            request.Format,
            request.StartsAtUtc,
            request.EndsAtUtc);

        communityEvent.SetLocation(request.Location);
        communityEvent.SetOnlineUrl(request.OnlineUrl);
        communityEvent.SetCapacity(request.Capacity);
        communityEvent.SetRegistrationClosesAt(request.RegistrationClosesAtUtc);
        communityEvent.SetCoverImage(request.CoverImageUrl);

        var publie = request.Publish;

        if (publie)
        {
            communityEvent.Publish(clock.GetUtcNow().UtcDateTime);
        }

        unitOfWork.Repository<CommunityEvent>().Add(communityEvent);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (publie)
        {
            await newsletter.DiffuserAsync(NouvellePublication.De(communityEvent), cancellationToken);
        }

        return CreatedAtAction(nameof(Lire), new { id = communityEvent.Id }, ApiProjections.ToDetail(communityEvent, 0));
    }

    /// <summary>Met à jour un événement, y compris son image de couverture.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="request">Nouveau contenu.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement mis à jour.</returns>
    /// <response code="200">Événement mis à jour.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Événement introuvable.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> MettreAJour(
        Guid id,
        [FromBody] UpdateEventRequest request,
        CancellationToken cancellationToken)
    {
        var communityEvent = await ChargerEvenement(id, cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        communityEvent.UpdateDetails(
            request.Title,
            request.Summary,
            request.Description,
            request.StartsAtUtc,
            request.EndsAtUtc);

        communityEvent.SetLocation(request.Location);
        communityEvent.SetOnlineUrl(request.OnlineUrl);
        communityEvent.SetCapacity(request.Capacity);
        communityEvent.SetRegistrationClosesAt(request.RegistrationClosesAtUtc);
        communityEvent.SetCoverImage(request.CoverImageUrl);

        // Seule la modification d'un événement déjà annoncé concerne des personnes : un
        // brouillon n'a personne à prévenir, et l'information sur sa nouvelle date n'est
        // encore connue de personne.
        var dejaPublie = communityEvent.Status == PublicationStatus.Published;

        unitOfWork.Repository<CommunityEvent>().Update(communityEvent);

        if (dejaPublie)
        {
            await PrevenirModificationAsync(communityEvent, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToDetail(communityEvent, await ConfirmedCountAsync(id, cancellationToken)));
    }

    /// <summary>Publie un événement en brouillon.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement publié.</returns>
    /// <response code="200">Événement publié.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Événement introuvable.</response>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> Publier(Guid id, CancellationToken cancellationToken)
    {
        var communityEvent = await ChargerEvenement(id, cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        var dejaPublie = communityEvent.PublishedAtUtc is not null;
        communityEvent.Publish(clock.GetUtcNow().UtcDateTime);

        unitOfWork.Repository<CommunityEvent>().Update(communityEvent);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!dejaPublie)
        {
            await newsletter.DiffuserAsync(NouvellePublication.De(communityEvent), cancellationToken);
        }

        return Ok(ApiProjections.ToDetail(communityEvent, await ConfirmedCountAsync(id, cancellationToken)));
    }

    /// <summary>Archive un événement, le retirant du calendrier sans le supprimer.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement archivé.</returns>
    /// <response code="200">Événement archivé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Événement introuvable.</response>
    [HttpPost("{id:guid}/archive")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> Archiver(Guid id, CancellationToken cancellationToken)
    {
        var communityEvent = await ChargerEvenement(id, cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        communityEvent.Archive();

        unitOfWork.Repository<CommunityEvent>().Update(communityEvent);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToDetail(communityEvent, await ConfirmedCountAsync(id, cancellationToken)));
    }

    /// <summary>Retire un événement du site sans en perdre l'historique.</summary>
    /// <param name="id">Identifiant de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <remarks>
    /// Les inscriptions déjà prises sont conservées : elles font partie de l'historique de
    /// l'événement, et les participants doivent pouvoir retrouver l'événement sur lequel
    /// ils ont commenté.
    /// </remarks>
    /// <response code="204">Événement retiré.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Événement introuvable.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken cancellationToken)
    {
        var communityEvent = await ChargerEvenement(id, cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        var auteur = RequireAdministrateur(currentUser);
        var maintenant = clock.GetUtcNow().UtcDateTime;

        // Un événement retiré n'aura pas lieu : les personnes inscrites doivent l'apprendre
        // de l'administration et non enconstater le jour venu.
        await PrevenirAnnulationAsync(communityEvent, cancellationToken);

        unitOfWork.SoftDeletableRepository<CommunityEvent>().Retire(communityEvent, maintenant, auteur);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private Task<CommunityEvent?> ChargerEvenement(Guid id, CancellationToken cancellationToken) =>
        unitOfWork.ReadRepository<CommunityEvent>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

    private Task<int> ConfirmedCountAsync(Guid eventId, CancellationToken cancellationToken) =>
        unitOfWork.ReadRepository<EventRegistration>().Query
            .CountAsync(
                inscription => inscription.CommunityEventId == eventId
                    && inscription.Status == EventRegistrationStatus.Confirmed,
                cancellationToken);

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
