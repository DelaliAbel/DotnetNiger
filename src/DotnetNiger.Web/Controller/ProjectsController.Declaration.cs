using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
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

/// <summary>Déclaration et mise à jour d'un projet.</summary>
public sealed partial class ProjectsController
{
    /// <summary>Déclare un projet et le publie aussitôt.</summary>
    /// <param name="request">Contenu du projet.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le projet créé.</returns>
    /// <response code="201">Projet créé et publié.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
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

        project.SetDemoUrl(request.DemoUrl);
        project.SetTechnologies(request.Technologies);
        project.SetImageUrl(request.ImageUrl);

        // Un projet déclaré est un projet voulu en ligne : il n'y a pas de file de relecture
        // entre la déclaration et la publication, et laisser un projet en attente n'aurait
        // personne pour le sortir de là.
        project.Publish(clock.GetUtcNow().UtcDateTime);

        unitOfWork.Repository<Project>().Add(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await newsletter.DiffuserAsync(NouvellePublication.De(project), cancellationToken);

        return CreatedAtAction(nameof(Lister), null, ApiProjections.ToSummary(project));
    }

    /// <summary>Met à jour un projet, y compris son image de présentation.</summary>
    /// <param name="id">Identifiant du projet.</param>
    /// <param name="request">Nouveau contenu.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le projet mis à jour.</returns>
    /// <response code="200">Projet mis à jour.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Projet introuvable.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
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
}
