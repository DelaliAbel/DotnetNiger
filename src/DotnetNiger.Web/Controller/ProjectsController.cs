using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
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
public sealed class ProjectsController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : ApiControllerBase
{
    /// <summary>Liste les projets publiés.</summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés de projets.</returns>
    /// <response code="200">Page de projets.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PageResponse<ProjectSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResponse<ProjectSummaryResponse>>> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var requete = unitOfWork.ReadRepository<Project>().Query
            .Where(project => project.Status == PublicationStatus.Published)
            .OrderByDescending(project => project.PublishedAtUtc)
            .ThenBy(project => project.Name);

        return await PageOfAsync(
            requete,
            new PageRequest(page, pageSize),
            ApiProjections.ToSummary,
            cancellationToken);
    }

    /// <summary>Déclare un projet, en attente de publication.</summary>
    /// <param name="request">Contenu du projet.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le projet créé.</returns>
    /// <response code="201">Projet créé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType<ProjectSummaryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProjectSummaryResponse>> Creer(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var auteur = RequireUser(currentUser);
        var project = new Project(
            auteur,
            request.Name,
            request.Summary,
            request.Description,
            request.RepositoryUrl);

        project.SetTechnologies(request.Technologies);
        project.SetImageUrl(request.ImageUrl);

        unitOfWork.Repository<Project>().Add(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Lister), null, ApiProjections.ToSummary(project));
    }
}
