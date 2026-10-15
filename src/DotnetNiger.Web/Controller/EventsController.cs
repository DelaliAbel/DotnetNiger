using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Queries;
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
/// <remarks>
/// Ce fichier ne porte que la lecture publique et les deux primitives de lecture dont se
/// servent les autres parties : les inscriptions d'un côté, l'administration de l'autre.
/// </remarks>
[Route("api/events")]
[Tags("Événements")]
public sealed partial class EventsController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IEmailDispatcher emails,
    IMemberDirectory members,
    INewsletterDiffusion newsletter,
    IOptions<EmailOptions> emailOptions,
    EvenementQueries lectures,
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
        // Le filtrage, le tri et le décompte des inscrits appartiennent au service de
        // lecture, que l'agenda du site utilise aussi : la page et l'API annoncent donc
        // les mêmes rencontres, dans le même ordre, avec le même nombre de places prises.
        var (evenements, total) = await lectures.ListerAsync(
            aVenir,
            format,
            recherche,
            trier,
            page,
            pageSize,
            cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<EvenementLu>(evenements, page, pageSize, total),
            ApiProjections.ToSummary);
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

    /// <summary>Lit un événement publié par son adresse lisible.</summary>
    /// <param name="slug">Adresse lisible de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement et son nombre d'inscriptions.</returns>
    /// <response code="200">Événement trouvé.</response>
    /// <response code="404">Événement introuvable ou non publié.</response>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType<EventSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventSummaryResponse>> LireParSlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var communityEvent = await unitOfWork.ReadRepository<CommunityEvent>().Query
            .FirstOrDefaultAsync(
                candidat => candidat.Slug == slug && candidat.Status == PublicationStatus.Published,
                cancellationToken);

        if (communityEvent is null)
        {
            return NotFound();
        }

        return Ok(ApiProjections.ToSummary(
            communityEvent,
            await ConfirmedCountAsync(communityEvent.Id, cancellationToken)));
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
}