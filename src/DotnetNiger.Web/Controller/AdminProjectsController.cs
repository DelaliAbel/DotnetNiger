using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
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
/// Gestion des projets de la communauté.
/// </summary>
/// <remarks>
/// La liste publique ne montre que les projets publiés : un projet en attente de publication
/// n'apparaît pas au catalogue. L'administration a donc sa propre liste, où états et
/// technologies coexistent.
/// </remarks>
[Route("api/admin/projects")]
public sealed class AdminProjectsController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : ApiControllerBase
{
    /// <summary>
    /// Liste tous les projets, quel que soit leur état de publication.
    /// </summary>
    /// <param name="statut">État à filtrer, ou <see langword="null"/> pour tous.</param>
    /// <param name="recherche">Filtre facultatif sur le nom, le résumé ou les technologies.</param>
    /// <param name="technologies">Filtre facultatif sur les technologies (sous-chaîne).</param>
    /// <param name="trier">Ordre facultatif : <c>recent</c>, <c>ancien</c>, <c>titre</c> ou <c>titre-desc</c>.</param>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés de projets.</returns>
    /// <response code="200">Projets listés.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("")]
    [ProducesResponseType<PageResponse<AdminProjectSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PageResponse<AdminProjectSummaryResponse>>> Lister(
        [FromQuery] PublicationStatus? statut = null,
        [FromQuery] string? recherche = null,
        [FromQuery] string? technologies = null,
        [FromQuery] string? trier = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        RequireAdministrateur(currentUser);

        var requete = unitOfWork.ReadRepository<Project>().Query;

        if (statut is { } etat)
        {
            requete = requete.Where(project => project.Status == etat);
        }

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var terme = recherche.Trim();
            requete = requete.Where(project =>
                project.Name.Contains(terme)
                || project.Summary.Contains(terme)
                || (project.Technologies != null && project.Technologies.Contains(terme)));
        }

        if (!string.IsNullOrWhiteSpace(technologies))
        {
            var terme = technologies.Trim();
            requete = requete.Where(project =>
                project.Technologies != null && project.Technologies.Contains(terme));
        }

        // Les projets n'ont pas d'horodatage de dernière modification : l'administration
        // classe par publication récente, puis par nom pour les états qui n'en ont pas.
        requete = trier switch
        {
            "ancien" => requete
                .OrderBy(project => project.PublishedAtUtc)
                .ThenBy(project => project.Name),
            "titre" => requete.OrderBy(project => project.Name),
            "titre-desc" => requete.OrderByDescending(project => project.Name),
            _ => requete
                .OrderByDescending(project => project.PublishedAtUtc)
                .ThenBy(project => project.Name),
        };

        return await ProjectPageOfAsync(
            requete,
            new PageRequest(page, pageSize),
            unitOfWork,
            ApiProjections.ToAdminSummary,
            cancellationToken);
    }
}