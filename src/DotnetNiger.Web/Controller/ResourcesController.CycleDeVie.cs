using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>Cycle de vie d'une ressource : archivage, brouillon, publication, retrait.</summary>
public sealed partial class ResourcesController
{
    [HttpPost("{id:guid}/publish")]
    [Authorize]
    [ProducesResponseType<ResourceSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceSummaryResponse>> Publier(
        Guid id,
        CancellationToken cancellationToken)
    {
        var resource = await unitOfWork.ReadRepository<Resource>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (resource is null)
        {
            return NotFound();
        }

        RequireAuteurOuAdministrateur(currentUser, resource.CreatorId);

        var dejaPublie = resource.PublishedAtUtc is not null;
        resource.Publish(clock.GetUtcNow().UtcDateTime);

        unitOfWork.Repository<Resource>().Update(resource);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!dejaPublie)
        {
            await newsletter.DiffuserAsync(NouvellePublication.De(resource), cancellationToken);
        }

        return Ok(ApiProjections.ToSummary(resource));
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize]
    [ProducesResponseType<ResourceSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceSummaryResponse>> Archiver(
        Guid id,
        CancellationToken cancellationToken)
    {
        var resource = await unitOfWork.ReadRepository<Resource>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (resource is null)
        {
            return NotFound();
        }

        RequireAuteurOuAdministrateur(currentUser, resource.CreatorId);

        resource.Archive();

        unitOfWork.Repository<Resource>().Update(resource);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToSummary(resource));
    }

    [HttpPost("{id:guid}/draft")]
    [Authorize]
    [ProducesResponseType<ResourceSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceSummaryResponse>> RepasserEnBrouillon(
        Guid id,
        CancellationToken cancellationToken)
    {
        var resource = await unitOfWork.ReadRepository<Resource>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (resource is null)
        {
            return NotFound();
        }

        RequireAuteurOuAdministrateur(currentUser, resource.CreatorId);

        resource.ReturnToDraft();

        unitOfWork.Repository<Resource>().Update(resource);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToSummary(resource));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken cancellationToken)
    {
        var resource = await unitOfWork.ReadRepository<Resource>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (resource is null)
        {
            return NotFound();
        }

        var auteur = RequireAuteurOuAdministrateur(currentUser, resource.CreatorId);

        unitOfWork.SoftDeletableRepository<Resource>().Retire(
            resource,
            clock.GetUtcNow().UtcDateTime,
            auteur);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
