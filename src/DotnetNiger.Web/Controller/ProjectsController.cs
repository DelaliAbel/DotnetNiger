using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Queries;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Projets open source de la communauté.
/// </summary>
[Route("api/projects")]
[Tags("Projets")]
public sealed partial class ProjectsController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    INewsletterDiffusion newsletter,
    CatalogueQueries lectures,
    TimeProvider clock) : ApiControllerBase
{
    /// <summary>Liste les projets publiés.</summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="recherche">Filtre facultatif sur le nom, le résumé ou les technologies.</param>
    /// <param name="technologies">Filtre facultatif sur les technologies (sous-chaîne).</param>
    /// <param name="trier">Ordre facultatif : <c>recent</c>, <c>ancien</c>, <c>titre</c> ou <c>titre-desc</c>.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés de projets.</returns>
    /// <response code="200">Page de projets.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PageResponse<ProjectSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResponse<ProjectSummaryResponse>>> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? recherche = null,
        [FromQuery] string? technologies = null,
        [FromQuery] string? trier = null,
        CancellationToken cancellationToken = default)
    {
        // Le filtrage et le tri appartiennent au service de lecture, que la page
        // communauté utilise aussi.
        var (projets, total) = await lectures.ListerProjetsAsync(
            recherche,
            technologies,
            trier,
            page,
            pageSize,
            cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<ProjetLu>(projets, page, pageSize, total),
            ApiProjections.ToSummary);
    }

    /// <summary>Lit un projet publié par son slug.</summary>
    /// <param name="slug">Slug du projet.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le projet demandé.</returns>
    /// <response code="200">Projet trouvé.</response>
    /// <response code="404">Projet introuvable ou non publié.</response>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType<ProjectSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectSummaryResponse>> Lire(
        string slug,
        CancellationToken cancellationToken)
    {
        var project = await unitOfWork.ReadRepository<Project>().Query
            .FirstOrDefaultAsync(
                candidat =>
                    candidat.Slug == slug && candidat.Status == PublicationStatus.Published,
                cancellationToken);

        return project is null ? NotFound() : Ok(ApiProjections.ToSummary(project));
    }
}
