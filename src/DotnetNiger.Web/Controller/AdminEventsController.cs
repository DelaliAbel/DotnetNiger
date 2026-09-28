using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Gestion des événements de la communauté.
/// </summary>
/// <remarks>
/// La liste publique ne montre que les événements publiés, et seulement ceux qui sont à venir
/// par défaut : un brouillon ou une édition annulée y serait invisible. L'administration a
/// donc sa propre liste, où états, formats et dates coexistent.
/// </remarks>
[Route("api/admin/events")]
public sealed class AdminEventsController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : ApiControllerBase
{
    /// <summary>
    /// Liste tous les événements, quel que soit leur état de publication.
    /// </summary>
    /// <param name="statut">État à filtrer, ou <see langword="null"/> pour tous.</param>
    /// <param name="format">Filtre facultatif par format.</param>
    /// <param name="aVenir">Ne garder que les événements non terminés.</param>
    /// <param name="recherche">Filtre facultatif sur le titre ou le résumé.</param>
    /// <param name="trier">Ordre facultatif : <c>date</c>, <c>date-desc</c>, <c>recent</c>, <c>ancien</c>, <c>titre</c> ou <c>titre-desc</c>.</param>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés d'événements.</returns>
    /// <response code="200">Événements listés.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("")]
    [ProducesResponseType<PageResponse<AdminEventSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PageResponse<AdminEventSummaryResponse>>> Lister(
        [FromQuery] PublicationStatus? statut = null,
        [FromQuery] CommunityEventFormat? format = null,
        [FromQuery] bool? aVenir = null,
        [FromQuery] string? recherche = null,
        [FromQuery] string? trier = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        RequireAdministrateur(currentUser);

        var requete = unitOfWork.ReadRepository<CommunityEvent>().Query;

        if (statut is { } etat)
        {
            requete = requete.Where(communityEvent => communityEvent.Status == etat);
        }

        if (format is { } valeur)
        {
            requete = requete.Where(communityEvent => communityEvent.Format == valeur);
        }

        if (aVenir is true)
        {
            requete = requete.Where(communityEvent => communityEvent.EndsAtUtc >= DateTime.UtcNow);
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

        return await EventPageOfAsync(
            requete,
            new PageRequest(page, pageSize),
            unitOfWork,
            ApiProjections.ToAdminSummary,
            cancellationToken);
    }
}