using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Partners;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Queries;
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
public sealed class PartnersController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    CatalogueQueries lectures,
    TimeProvider clock) : ApiControllerBase
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
        // La sélection et l'ordre appartiennent au service de lecture, que la page
        // partenaires utilise aussi : la page et l'API ne peuvent pas proposer des
        // partenaires dans deux ordres différents.
        var partenaires = await lectures.ListerPartenairesAsync(tier, cancellationToken);

        return Ok(partenaires.Select(ApiProjections.ToResponse).ToList());
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

    /// <summary>Ajoute un partenaire, pour l'administration.</summary>
    /// <param name="request">Contenu du partenaire.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le partenaire créé.</returns>
    /// <response code="201">Partenaire créé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<PartnerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PartnerResponse>> Creer(
        [FromBody] CreatePartnerRequest request,
        CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var partner = new Partner(
            request.Name,
            request.Description,
            request.WebsiteUrl,
            request.Tier);

        partner.SetLogoUrl(request.LogoUrl);
        partner.SetSortOrder(request.SortOrder);

        unitOfWork.Repository<Partner>().Add(partner);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Lire), new { slug = partner.Slug }, ApiProjections.ToResponse(partner));
    }

    /// <summary>Met à jour un partenaire, pour l'administration.</summary>
    /// <param name="id">Identifiant du partenaire.</param>
    /// <param name="request">Nouveau contenu.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le partenaire mis à jour.</returns>
    /// <response code="200">Partenaire mis à jour.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Partenaire introuvable.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<PartnerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartnerResponse>> MettreAJour(
        Guid id,
        [FromBody] UpdatePartnerRequest request,
        CancellationToken cancellationToken)
    {
        var partner = await ChargerPartenaire(id, cancellationToken);

        if (partner is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        partner.UpdateDetails(request.Name, request.Description, request.WebsiteUrl);
        partner.SetLogoUrl(request.LogoUrl);
        partner.ChangeTier(request.Tier);
        partner.SetSortOrder(request.SortOrder);

        unitOfWork.Repository<Partner>().Update(partner);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToResponse(partner));
    }

    /// <summary>Active ou retire un partenaire de la page des sponsors.</summary>
    /// <param name="id">Identifiant du partenaire.</param>
    /// <param name="actif">Vrai pour afficher, faux pour masquer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le partenaire avec son nouvel état.</returns>
    /// <remarks>
    /// Un partenaire n'est pas effacé lorsqu'un sponsoring prend fin : il est simplement
    /// retiré de l'affichage, et l'historique des événements reste rattaché.
    /// </remarks>
    /// <response code="200">Partenaire mis à jour.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Partenaire introuvable.</response>
    [HttpPost("{id:guid}/activation")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<PartnerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartnerResponse>> ChangerActivation(
        Guid id,
        [FromQuery] bool actif,
        CancellationToken cancellationToken)
    {
        var partner = await ChargerPartenaire(id, cancellationToken);

        if (partner is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        if (actif)
        {
            partner.Activate();
        }
        else
        {
            partner.Deactivate();
        }

        unitOfWork.Repository<Partner>().Update(partner);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToResponse(partner));
    }

    /// <summary>Retire définitivement un partenaire, pour l'administration.</summary>
    /// <param name="id">Identifiant du partenaire.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <response code="204">Partenaire retiré.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Partenaire introuvable.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken cancellationToken)
    {
        var partner = await ChargerPartenaire(id, cancellationToken);

        if (partner is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        unitOfWork.SoftDeletableRepository<Partner>().Retire(
            partner,
            clock.GetUtcNow().UtcDateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private Task<Partner?> ChargerPartenaire(Guid id, CancellationToken cancellationToken) =>
        unitOfWork.ReadRepository<Partner>().Query
            .FirstOrDefaultAsync(partner => partner.Id == id, cancellationToken);
}
