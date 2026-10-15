using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>Dépôt et mise à jour d'une ressource partagée.</summary>
public sealed partial class ResourcesController
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType<ResourceSummaryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResourceSummaryResponse>> Creer(
        [FromBody] CreateResourceRequest request,
        CancellationToken cancellationToken)
    {
        var auteur = RequireUser(currentUser);

        var resource = await depot.DeposerAsync(
            auteur,
            request.Title,
            request.Description,
            request.Category,
            request.ExternalUrl,
            request.Provider,
            request.CoverImageUrl,
            request.Content,
            cancellationToken);

        return CreatedAtAction(nameof(Lister), null, ApiProjections.ToSummary(resource));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType<ResourceSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceSummaryResponse>> MettreAJour(
        Guid id,
        [FromBody] UpdateResourceRequest request,
        CancellationToken cancellationToken)
    {
        var resource = await unitOfWork.ReadRepository<Resource>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (resource is null)
        {
            return NotFound();
        }

        RequireAuteurOuAdministrateur(currentUser, resource.CreatorId);

        resource.UpdateDetails(request.Title, request.Description, request.Category);
        resource.SetContent(request.Content);
        resource.SetCoverImage(request.CoverImageUrl);

        if (!string.IsNullOrWhiteSpace(request.ExternalUrl))
        {
            resource.LinkTo(request.ExternalUrl, request.Provider);
        }

        unitOfWork.Repository<Resource>().Update(resource);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToSummary(resource));
    }
}
