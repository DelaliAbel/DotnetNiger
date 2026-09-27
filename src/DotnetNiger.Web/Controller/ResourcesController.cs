using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
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
public sealed class ResourcesController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    TimeProvider clock) : ApiControllerBase
{
    /// <summary>Liste les ressources publiées.</summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="categorie">Filtre facultatif par catégorie.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés de ressources.</returns>
    /// <response code="200">Page de ressources.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PageResponse<ResourceSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResponse<ResourceSummaryResponse>>> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ResourceCategory? categorie = null,
        CancellationToken cancellationToken = default)
    {
        var requete = unitOfWork.ReadRepository<Resource>().Query
            .Where(resource => resource.Status == PublicationStatus.Published);

        if (categorie is { } valeur)
        {
            requete = requete.Where(resource => resource.Category == valeur);
        }

        requete = requete
            .OrderByDescending(resource => resource.PublishedAtUtc)
            .ThenBy(resource => resource.Title);

        return await PageOfAsync(
            requete,
            new PageRequest(page, pageSize),
            ApiProjections.ToSummary,
            cancellationToken);
    }

    /// <summary>Propose une ressource, publiée immédiatement.</summary>
    /// <param name="request">Contenu de la ressource.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La ressource créée.</returns>
    /// <remarks>
    /// Une ressource proposée par un membre est publiée d'emblée : c'est la règle du site,
    /// qui n'exige pas de validation préalable pour ce type de contribution.
    /// </remarks>
    /// <response code="201">Ressource créée.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType<ResourceSummaryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResourceSummaryResponse>> Creer(
        [FromBody] CreateResourceRequest request,
        CancellationToken cancellationToken)
    {
        var auteur = RequireUser(currentUser);
        var resource = new Resource(auteur, request.Title, request.Description, request.Category);

        if (!string.IsNullOrWhiteSpace(request.ExternalUrl))
        {
            resource.LinkTo(request.ExternalUrl, request.Provider);
        }

        if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
        {
            resource.SetCoverImage(request.CoverImageUrl);
        }

        resource.Publish(clock.GetUtcNow().UtcDateTime);

        unitOfWork.Repository<Resource>().Add(resource);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Lister), null, ApiProjections.ToSummary(resource));
    }
}
