using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Email;
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
    INewsletterDiffusion newsletter,
    TimeProvider clock) : ApiControllerBase
{
    /// <summary>Liste les ressources publiées.</summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="categorie">Filtre facultatif par catégorie.</param>
    /// <param name="recherche">Filtre facultatif sur le titre, la description ou le fournisseur.</param>
    /// <param name="trier">Ordre facultatif : <c>recent</c>, <c>ancien</c>, <c>titre</c> ou <c>titre-desc</c>.</param>
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
        [FromQuery] string? recherche = null,
        [FromQuery] string? trier = null,
        CancellationToken cancellationToken = default)
    {
        var requete = unitOfWork.ReadRepository<Resource>().Query
            .Where(resource => resource.Status == PublicationStatus.Published);

        if (categorie is { } valeur)
        {
            requete = requete.Where(resource => resource.Category == valeur);
        }

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var terme = recherche.Trim();
            requete = requete.Where(resource =>
                resource.Title.Contains(terme)
                || resource.Description.Contains(terme)
                || (resource.Provider != null && resource.Provider.Contains(terme)));
        }

        requete = trier switch
        {
            "ancien" => requete
                .OrderBy(resource => resource.PublishedAtUtc)
                .ThenBy(resource => resource.Title),
            "titre" => requete.OrderBy(resource => resource.Title),
            "titre-desc" => requete.OrderByDescending(resource => resource.Title),
            _ => requete
                .OrderByDescending(resource => resource.PublishedAtUtc)
                .ThenBy(resource => resource.Title),
        };

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

        await newsletter.DiffuserAsync(NouvellePublication.De(resource), cancellationToken);

        return CreatedAtAction(nameof(Lister), null, ApiProjections.ToSummary(resource));
    }

    /// <summary>Lit une ressource publiée par son slug.</summary>
    /// <param name="slug">Slug de la ressource.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La ressource demandée.</returns>
    /// <response code="200">Ressource trouvée.</response>
    /// <response code="404">Ressource introuvable ou non publiée.</response>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType<ResourceSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceSummaryResponse>> Lire(
        string slug,
        CancellationToken cancellationToken)
    {
        var resource = await unitOfWork.ReadRepository<Resource>().Query
            .FirstOrDefaultAsync(
                candidat =>
                    candidat.Slug == slug && candidat.Status == PublicationStatus.Published,
                cancellationToken);

        return resource is null ? NotFound() : Ok(ApiProjections.ToSummary(resource));
    }

    /// <summary>Met à jour une ressource, y compris son image de couverture.</summary>
    /// <param name="id">Identifiant de la ressource.</param>
    /// <param name="request">Nouveau contenu.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La ressource mise à jour.</returns>
    /// <response code="200">Ressource mise à jour.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Ressource introuvable.</response>
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

    /// <summary>Archive une ressource, la retirant du site sans la supprimer.</summary>
    /// <param name="id">Identifiant de la ressource.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La ressource archivée.</returns>
    /// <response code="200">Ressource archivée.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Ressource introuvable.</response>
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

    /// <summary>Repasse une ressource en brouillon, la retirant du site sans la supprimer.</summary>
    /// <param name="id">Identifiant de la ressource.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La ressource repassée en brouillon.</returns>
    /// <response code="200">Ressource repassée en brouillon.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Ressource introuvable.</response>
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

    /// <summary>Republie une ressource archivée.</summary>
    /// <param name="id">Identifiant de la ressource.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La ressource republiée.</returns>
    /// <response code="200">Ressource republiée.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Ressource introuvable.</response>
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

    /// <summary>Retire une ressource du site sans en perdre l'historique.</summary>
    /// <param name="id">Identifiant de la ressource.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <response code="204">Ressource retirée.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Ressource introuvable.</response>
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
