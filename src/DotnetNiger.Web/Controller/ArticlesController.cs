using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Articles du blog.
/// </summary>
[Route("api/articles")]
[Tags("Articles")]
public sealed class ArticlesController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    TimeProvider clock) : ApiControllerBase
{
    /// <summary>Liste les articles publiés.</summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="categorie">Filtre facultatif par catégorie.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés d'articles.</returns>
    /// <response code="200">Page d'articles.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PageResponse<ArticleSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResponse<ArticleSummaryResponse>>> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ArticleCategory? categorie = null,
        CancellationToken cancellationToken = default)
    {
        var requete = unitOfWork.ReadRepository<Article>().Query
            .Where(article => article.Status == PublicationStatus.Published);

        if (categorie is { } valeur)
        {
            requete = requete.Where(article => article.Category == valeur);
        }

        requete = requete
            .OrderByDescending(article => article.PublishedAtUtc)
            .ThenBy(article => article.Title);

        return await ArticlePageOfAsync(
            requete,
            new PageRequest(page, pageSize),
            unitOfWork,
            cancellationToken);
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
        var article = await unitOfWork.ReadRepository<Article>().Query
            .FirstOrDefaultAsync(
                candidat => candidat.Slug == slug && candidat.Status == PublicationStatus.Published,
                cancellationToken);

        if (article is null)
        {
            return NotFound();
        }

        var auteurs = await NomsAsync(unitOfWork, [article.AuthorId], cancellationToken);

        return Ok(ApiProjections.ToDetail(
            article,
            auteurs.GetValueOrDefault(article.AuthorId, string.Empty)));
    }

    /// <summary>Propose un article, en brouillon ou publié.</summary>
    /// <param name="request">Contenu de l'article.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'article créé.</returns>
    /// <response code="201">Article créé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ArticleResponse>> Creer(
        [FromBody] CreateArticleRequest request,
        CancellationToken cancellationToken)
    {
        var auteur = RequireUser(currentUser);
        var article = new Article(
            auteur,
            request.Title,
            request.Excerpt,
            request.Content,
            request.Category);

        if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
        {
            article.SetCoverImage(request.CoverImageUrl);
        }

        if (request.Publish)
        {
            article.Publish(clock.GetUtcNow().UtcDateTime);
        }

        unitOfWork.Repository<Article>().Add(article);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Lire), new { slug = article.Slug }, ApiProjections.ToDetail(article, string.Empty));
    }
}
