using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Création et mise à jour d'un article.
/// </summary>
public sealed partial class ArticlesController
{
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
        var auteur = RequireAdministrateur(currentUser);
        var rubrique = await TrouverRubriqueAsync(request.CategoryId, cancellationToken);
        var article = new Article(
            auteur,
            request.Title,
            request.Excerpt,
            request.Content,
            rubrique);

        if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
        {
            article.SetCoverImage(request.CoverImageUrl);
        }

        var publie = request.Publish;

        if (publie)
        {
            article.Publish(clock.GetUtcNow().UtcDateTime);
        }

        unitOfWork.Repository<Article>().Add(article);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (publie)
        {
            await newsletter.DiffuserAsync(NouvellePublication.De(article), cancellationToken);
        }

        return CreatedAtAction(nameof(Lire), new { slug = article.Slug }, ApiProjections.ToDetail(article, string.Empty));
    }

    /// <summary>Met à jour un article, y compris son image de couverture.</summary>
    /// <param name="id">Identifiant de l'article.</param>
    /// <param name="request">Nouveau contenu.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'article mis à jour.</returns>
    /// <response code="200">Article mis à jour.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Article introuvable.</response>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleResponse>> MettreAJour(
        Guid id,
        [FromBody] UpdateArticleRequest request,
        CancellationToken cancellationToken)
    {
        var article = await unitOfWork.ReadRepository<Article>().Query
            .Include(article => article.Rubrique)
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (article is null)
        {
            return NotFound();
        }

        RequireAdministrateur(currentUser);

        var rubrique = await TrouverRubriqueAsync(request.CategoryId, cancellationToken);
        article.UpdateDetails(request.Title, request.Excerpt, request.Content, rubrique);
        article.SetCoverImage(request.CoverImageUrl);

        unitOfWork.Repository<Article>().Update(article);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var auteurs = await NomsAsync(unitOfWork, [article.AuthorId], cancellationToken);

        return Ok(ApiProjections.ToDetail(
            article,
            auteurs.GetValueOrDefault(article.AuthorId, string.Empty)));
    }
}