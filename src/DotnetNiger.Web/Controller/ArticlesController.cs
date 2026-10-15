using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Queries;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Articles du blog.
/// </summary>
/// <remarks>
/// Les écritures sont réparties dans les parties voisines : ce fichier ne porte que la
/// lecture, c'est-à-dire ce que le blog public consomme.
/// </remarks>
[Route("api/articles")]
[Tags("Articles")]
public sealed partial class ArticlesController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    INewsletterDiffusion newsletter,
    ArticleQueries lectures,
    TimeProvider clock) : ApiControllerBase
{
    /// <summary>Liste les articles publiés.</summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="rubrique">Filtre facultatif par rubrique.</param>
    /// <param name="recherche">Filtre facultatif sur le titre ou le chapô.</param>
    /// <param name="trier">Ordre facultatif : <c>recent</c>, <c>ancien</c>, <c>titre</c> ou <c>titre-desc</c>.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés d'articles.</returns>
    /// <response code="200">Page d'articles.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PageResponse<ArticleSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResponse<ArticleSummaryResponse>>> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? rubrique = null,
        [FromQuery] string? recherche = null,
        [FromQuery] string? trier = null,
        CancellationToken cancellationToken = default)
    {
        // Le filtrage, le tri et la pagination appartiennent au service de lecture, que
        // les pages du site utilisent aussi : l'API et le blog ne peuvent donc pas
        // diverger sur ce qu'est un article publié, ni sur l'ordre dans lequel il sort.
        var (articles, total) = await lectures.ListerAsync(
            rubrique,
            recherche,
            trier,
            page,
            pageSize,
            cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<ArticleLu>(articles, page, pageSize, total),
            ApiProjections.ToSummary);
    }

    /// <summary>Lit un article publié par son slug.</summary>
    /// <param name="slug">Slug de l'article.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'article complet.</returns>
    /// <response code="200">Article trouvé.</response>
    /// <response code="404">Article introuvable ou non publié.</response>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleResponse>> Lire(
        string slug,
        CancellationToken cancellationToken)
    {
        var article = await lectures.LireAsync(slug, cancellationToken);

        if (article is null)
        {
            return NotFound();
        }

        return Ok(ApiProjections.ToDetail(article));
    }
}