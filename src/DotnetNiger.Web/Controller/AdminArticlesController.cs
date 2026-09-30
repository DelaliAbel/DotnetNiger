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
/// Gestion des actualités du site.
/// </summary>
/// <remarks>
/// La liste publique ne montre que les articles publiés : un brouillon qu'on relit ne doit
/// pas apparaître dans le flux, et l'y voir interdisait de vérifier un travail en cours.
/// L'administration a donc sa propre liste, où filtres et états coexistent.
/// </remarks>
[Route("api/admin/articles")]
public sealed class AdminArticlesController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : ApiControllerBase
{
    /// <summary>
    /// Liste tous les articles, quel que soit leur état de publication.
    /// </summary>
    /// <param name="statut">État à filtrer, ou <see langword="null"/> pour tous.</param>
    /// <param name="rubrique">Filtre facultatif par rubrique.</param>
    /// <param name="recherche">Filtre facultatif sur le titre ou le chapô.</param>
    /// <param name="trier">Ordre facultatif : <c>recent</c>, <c>ancien</c>, <c>titre</c> ou <c>titre-desc</c>.</param>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés d'articles.</returns>
    /// <response code="200">Articles listés.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("")]
    [ProducesResponseType<PageResponse<AdminArticleSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PageResponse<AdminArticleSummaryResponse>>> Lister(
        [FromQuery] PublicationStatus? statut = null,
        [FromQuery] Guid? rubrique = null,
        [FromQuery] string? recherche = null,
        [FromQuery] string? trier = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        RequireAdministrateur(currentUser);

        // La projection renvoie le libellé de la rubrique : elle doit être chargée.
        IQueryable<Article> requete = unitOfWork.ReadRepository<Article>().Query
            .Include(article => article.Rubrique);

        if (statut is { } etat)
        {
            requete = requete.Where(article => article.Status == etat);
        }

        if (rubrique is { } valeur)
        {
            requete = requete.Where(article => article.RubriqueId == valeur);
        }

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var terme = recherche.Trim();
            requete = requete.Where(article =>
                article.Title.Contains(terme) || article.Excerpt.Contains(terme));
        }

        // Ce que l'administration veut voir en premier, ce n'est pas l'ordre de publication :
        // c'est ce qu'elle vient de toucher. Un article modifié il y a dix minutes compte plus
        // qu'un autre publié le mois dernier.
        requete = trier switch
        {
            "ancien" => requete
                .OrderBy(article => article.UpdatedAtUtc ?? article.CreatedAtUtc)
                .ThenBy(article => article.Title),
            "titre" => requete.OrderBy(article => article.Title),
            "titre-desc" => requete.OrderByDescending(article => article.Title),
            _ => requete
                .OrderByDescending(article => article.UpdatedAtUtc ?? article.CreatedAtUtc)
                .ThenBy(article => article.Title),
        };

        return await ArticlePageOfAsync(
            requete,
            new PageRequest(page, pageSize),
            unitOfWork,
            ApiProjections.ToAdminSummary,
            cancellationToken);
    }
}
