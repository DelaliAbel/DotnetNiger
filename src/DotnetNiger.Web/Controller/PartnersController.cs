using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Partners;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Partenaires du projet.
/// </summary>
[Route("api/partners")]
[Tags("Partenaires")]
public sealed class PartnersController(IUnitOfWork unitOfWork) : ApiControllerBase
{
    /// <summary>Liste les partenaires actifs.</summary>
    /// <param name="tier">Filtre facultatif par niveau.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Partenaires actifs.</returns>
    /// <response code="200">Liste des partenaires.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<PartnerResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PartnerResponse>>> Lister(
        [FromQuery] PartnerTier? tier = null,
        CancellationToken cancellationToken = default)
    {
        var requete = unitOfWork.ReadRepository<Partner>().Query
            .Where(partner => partner.IsActive);

        if (tier is { } niveau)
        {
            requete = requete.Where(partner => partner.Tier == niveau);
        }

        // L'ordre d'affichage est fixé par l'administration, puis alphabétique pour que
        // deux partenaires d'un même niveau ne changent pas d'ordre d'une requête à l'autre.
        var items = await requete
            .OrderBy(partner => partner.SortOrder)
            .ThenBy(partner => partner.Name)
            .ToListAsync(cancellationToken);

        return Ok(items.Select(ApiProjections.ToResponse).ToList());
    }

    /// <summary>Lit un partenaire actif par son slug.</summary>
    /// <param name="slug">Slug du partenaire.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le partenaire.</returns>
    /// <response code="200">Partenaire trouvé.</response>
    /// <response code="404">Partenaire introuvable ou inactif.</response>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType<PartnerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartnerResponse>> Lire(string slug, CancellationToken cancellationToken)
    {
        var partenaire = await unitOfWork.ReadRepository<Partner>().Query
            .FirstOrDefaultAsync(
                candidat => candidat.Slug == slug && candidat.IsActive,
                cancellationToken);

        return partenaire is null ? NotFound() : Ok(ApiProjections.ToResponse(partenaire));
    }
}
