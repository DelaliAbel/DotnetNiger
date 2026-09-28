using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Gestion des ressources pédagogiques.
/// </summary>
/// <remarks>
/// La liste publique ne montre que les ressources publiées : un brouillon en cours de
/// rédaction n'apparaît pas dans le catalogue. L'administration a donc sa propre liste, où
/// états et catégories coexistent.
/// </remarks>
[Route("api/admin/resources")]
public sealed class AdminResourcesController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : ApiControllerBase
{
    /// <summary>
    /// Liste toutes les ressources, quel que soit leur état de publication.
    /// </summary>
    /// <param name="statut">État à filtrer, ou <see langword="null"/> pour tous.</param>
    /// <param name="categorie">Filtre facultatif par catégorie.</param>
    /// <param name="recherche">Filtre facultatif sur le titre, la description ou le fournisseur.</param>
    /// <param name="trier">Ordre facultatif : <c>recent</c>, <c>ancien</c>, <c>titre</c> ou <c>titre-desc</c>.</param>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés de ressources.</returns>
    /// <response code="200">Ressources listées.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("")]
    [ProducesResponseType<PageResponse<AdminResourceSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PageResponse<AdminResourceSummaryResponse>>> Lister(
        [FromQuery] PublicationStatus? statut = null,
        [FromQuery] ResourceCategory? categorie = null,
        [FromQuery] string? recherche = null,
        [FromQuery] string? trier = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        RequireAdministrateur(currentUser);

        var requete = unitOfWork.ReadRepository<Resource>().Query;

        if (statut is { } etat)
        {
            requete = requete.Where(resource => resource.Status == etat);
        }

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

        // Les ressources n'ont pas d'horodatage de dernière modification : l'administration
        // classe par publication récente, puis par titre pour les états qui n'en ont pas.
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

        return await ResourcePageOfAsync(
            requete,
            new PageRequest(page, pageSize),
            unitOfWork,
            ApiProjections.ToAdminSummary,
            cancellationToken);
    }
}