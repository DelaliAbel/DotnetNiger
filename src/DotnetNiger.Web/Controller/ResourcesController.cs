using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Content;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Queries;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Ressources partagées par la communauté.
/// </summary>
[Route("api/resources")]
[Tags("Ressources")]
public sealed partial class ResourcesController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    INewsletterDiffusion newsletter,
    CatalogueQueries lectures,
    DepotRessources depot,
    TimeProvider clock) : ApiControllerBase
{
    /// <summary>Liste les ressources publiées.</summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="categorie">Filtre facultatif par catégorie.</param>
    /// <param name="recherche">Filtre facultatif sur le titre, la description ou le fournisseur.</param>
    /// <param name="trier">Ordre facultatif : <c>recent</c>, <c>ancien</c>, <c>titre</c> ou <c>titre-desc</c>.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés de ressources.</returns>
    /// <response code="200">Page de ressources.</response>
    /// <response code="400">Pagination hors bornes.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PageResponse<ResourceSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PageResponse<ResourceSummaryResponse>>> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ResourceCategory? categorie = null,
        [FromQuery] string? recherche = null,
        [FromQuery] string? trier = null,
        CancellationToken cancellationToken = default)
    {
        // Le filtrage, le tri et la pagination appartiennent au service de lecture, que la
        // page publique /ressources consomme aussi : l'API et le site ne peuvent donc pas
        // diverger sur ce qu'est une ressource publiée ni sur l'ordre dans lequel elle sort.
        // Le service raisonne en pagination libre ; les bornes de l'API sont posées ici.
        var demande = new PageRequest(page, pageSize);

        var (ressources, total) = await lectures.ListerRessourcesAsync(
            categorie,
            recherche,
            trier,
            demande.PageNumber,
            demande.PageSize,
            cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<RessourceLu>(ressources, demande.PageNumber, demande.PageSize, total),
            ApiProjections.ToSummary);
    }

    /// <summary>Lit une ressource publiée par son slug.</summary>
    /// <param name="slug">Slug de la ressource.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La ressource demandée.</returns>
    /// <response code="200">Ressource trouvée.</response>
    /// <response code="404">Ressource introuvable ou non publiée.</response>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType<ResourceSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceSummaryResponse>> Lire(
        string slug,
        CancellationToken cancellationToken)
    {
        var resource = await unitOfWork.ReadRepository<Resource>().Query
            .FirstOrDefaultAsync(
                candidat =>
                    candidat.Slug == slug && candidat.Status == PublicationStatus.Published,
                cancellationToken);

        return resource is null ? NotFound() : Ok(ApiProjections.ToSummary(resource));
    }
}
