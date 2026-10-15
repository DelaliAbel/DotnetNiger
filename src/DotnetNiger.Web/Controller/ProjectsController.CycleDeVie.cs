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

/// <summary>Cycle de vie d'un projet : publication, archivage, retrait.</summary>
public sealed partial class ProjectsController
{
    /// <summary>Publie un projet en brouillon.</summary>
    /// <param name="id">Identifiant du projet.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le projet publié.</returns>
    /// <response code="200">Projet publié.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Projet introuvable.</response>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = RoleNames.Admin)]
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
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Projet introuvable.</response>
    [HttpPost("{id:guid}/archive")]
    [Authorize(Roles = RoleNames.Admin)]
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

    /// <summary>Repasse un projet en brouillon, le retirant du site sans le supprimer.</summary>
    /// <param name="id">Identifiant du projet.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le projet repassé en brouillon.</returns>
    /// <response code="200">Projet repassé en brouillon.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Projet introuvable.</response>
    [HttpPost("{id:guid}/draft")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<ProjectSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectSummaryResponse>> RepasserEnBrouillon(
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

        project.ReturnToDraft();

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
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Projet introuvable.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
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
