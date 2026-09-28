using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Email;
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
    ICurrentUserService currentUser,
    INewsletterDiffusion newsletter,
    TimeProvider clock) : ApiControllerBase
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
        var auteur = RequireAdministrateur(currentUser);
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

    /// <summary>Met à jour un projet, y compris son image de présentation.</summary>
    /// <param name="id">Identifiant du projet.</param>
    /// <param name="request">Nouveau contenu.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le projet mis à jour.</returns>
    /// <response code="200">Projet mis à jour.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Projet introuvable.</response>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType<ProjectSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectSummaryResponse>> MettreAJour(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var project = await unitOfWork.ReadRepository<Project>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        project.UpdateDetails(
            request.Name,
            request.Summary,
            request.Description,
            request.RepositoryUrl);

        project.SetDemoUrl(request.DemoUrl);
        project.SetTechnologies(request.Technologies);
        project.SetImageUrl(request.ImageUrl);

        unitOfWork.Repository<Project>().Update(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToSummary(project));
    }

    /// <summary>Publie un projet en brouillon.</summary>
    /// <param name="id">Identifiant du projet.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le projet publié.</returns>
    /// <response code="200">Projet publié.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Projet introuvable.</response>
    [HttpPost("{id:guid}/publish")]
    [Authorize]
    [ProducesResponseType<ProjectSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectSummaryResponse>> Publier(
        Guid id,
        CancellationToken cancellationToken)
    {
        var project = await unitOfWork.ReadRepository<Project>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        var dejaPublie = project.PublishedAtUtc is not null;
        project.Publish(clock.GetUtcNow().UtcDateTime);

        unitOfWork.Repository<Project>().Update(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!dejaPublie)
        {
            await newsletter.DiffuserAsync(NouvellePublication.De(project), cancellationToken);
        }

        return Ok(ApiProjections.ToSummary(project));
    }

    /// <summary>Archive un projet publié, le retirant du site sans le supprimer.</summary>
    /// <param name="id">Identifiant du projet.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le projet archivé.</returns>
    /// <response code="200">Projet archivé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Projet introuvable.</response>
    [HttpPost("{id:guid}/archive")]
    [Authorize]
    [ProducesResponseType<ProjectSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectSummaryResponse>> Archiver(
        Guid id,
        CancellationToken cancellationToken)
    {
        var project = await unitOfWork.ReadRepository<Project>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        project.Archive();

        unitOfWork.Repository<Project>().Update(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToSummary(project));
    }

    /// <summary>Retire un projet du site sans en perdre l'historique.</summary>
    /// <param name="id">Identifiant du projet.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <response code="204">Projet retiré.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Projet introuvable.</response>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken cancellationToken)
    {
        var project = await unitOfWork.ReadRepository<Project>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        var auteur = RequireAdministrateur(currentUser);

        unitOfWork.SoftDeletableRepository<Project>().Retire(
            project,
            clock.GetUtcNow().UtcDateTime,
            auteur);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
