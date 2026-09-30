using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Email;
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
    INewsletterDiffusion newsletter,
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
        // La rubrique est chargée : la projection renvoie son libellé, et une
        // navigation non chargée vaudrait null.
        // La rubrique est chargée : la projection renvoie son libellé, et une
        // navigation non chargée vaudrait null. Le type est explicite car la requête
        // reçoit ensuite des Where et des OrderBy.
        IQueryable<Article> requete = unitOfWork.ReadRepository<Article>().Query
            .Include(article => article.Rubrique)
            .Where(article => article.Status == PublicationStatus.Published);

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

        requete = trier switch
        {
            "ancien" => requete
                .OrderBy(article => article.PublishedAtUtc)
                .ThenBy(article => article.Title),
            "titre" => requete.OrderBy(article => article.Title),
            "titre-desc" => requete.OrderByDescending(article => article.Title),
            _ => requete
                .OrderByDescending(article => article.PublishedAtUtc)
                .ThenBy(article => article.Title),
        };

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
            .Include(article => article.Rubrique)
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

    /// <summary>Publie un article en brouillon.</summary>
    /// <param name="id">Identifiant de l'article.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'article publié.</returns>
    /// <response code="200">Article publié.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Article introuvable.</response>
    [HttpPost("{id:guid}/publish")]
    [Authorize]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleResponse>> Publier(
        Guid id,
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

        var dejaPublie = article.PublishedAtUtc is not null;
        article.Publish(clock.GetUtcNow().UtcDateTime);

        unitOfWork.Repository<Article>().Update(article);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!dejaPublie)
        {
            await newsletter.DiffuserAsync(NouvellePublication.De(article), cancellationToken);
        }

        return Ok(ApiProjections.ToDetail(article, string.Empty));
    }

    /// <summary>Archive un article publié, le retirant du site sans le supprimer.</summary>
    /// <param name="id">Identifiant de l'article.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'article archivé.</returns>
    /// <response code="200">Article archivé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Article introuvable.</response>
    [HttpPost("{id:guid}/archive")]
    [Authorize]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleResponse>> Archiver(
        Guid id,
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

        article.Archive();

        unitOfWork.Repository<Article>().Update(article);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToDetail(article, string.Empty));
    }

    /// <summary>Remet un article archivé ou publié en brouillon.</summary>
    /// <param name="id">Identifiant de l'article.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'article repassé en brouillon.</returns>
    /// <response code="200">Article repassé en brouillon.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Article introuvable.</response>
    [HttpPost("{id:guid}/draft")]
    [Authorize]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleResponse>> RepasserEnBrouillon(
        Guid id,
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

        article.ReturnToDraft();

        unitOfWork.Repository<Article>().Update(article);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToDetail(article, string.Empty));
    }

    /// <summary>Retire un article du site sans en perdre l'historique.</summary>
    /// <param name="id">Identifiant de l'article.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <response code="204">Article retiré.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est ni l'auteur ni un administrateur.</response>
    /// <response code="404">Article introuvable.</response>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken cancellationToken)
    {
        var article = await unitOfWork.ReadRepository<Article>().Query
            .Include(article => article.Rubrique)
            .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

        if (article is null)
        {
            return NotFound();
        }

        var auteur = RequireAdministrateur(currentUser);

        // Retrait logique : les commentaires et les signalements liés restent rattachés à
        // l'article, et l'historique de publication est conservé.
        unitOfWork.SoftDeletableRepository<Article>().Retire(
            article,
            clock.GetUtcNow().UtcDateTime,
            auteur);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Retrouve la rubrique désignée par son identifiant.
    /// </summary>
    /// <param name="rubriqueId">Identifiant transmis par le client.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La rubrique demandée.</returns>
    /// <exception cref="DomainException">La rubrique n'existe pas.</exception>
    /// <remarks>
    /// Le client envoie un identifiant et non un libellé : c'est le seul qui ne bouge pas
    /// quand la rubrique est renommée. Un identifiant inconnu est une erreur explicite,
    /// pas un article enregistré sous une rubrique au hasard.
    /// </remarks>
    private async Task<Rubrique> TrouverRubriqueAsync(Guid rubriqueId, CancellationToken cancellationToken)
    {
        if (rubriqueId == Guid.Empty)
        {
            throw new DomainException("categorie_requise", "Choisissez une rubrique.");
        }

        // La rubrique est chargée en suivi, et non via le dépôt de lecture. Celui-ci
        // applique `AsNoTracking` : la rubrique obtenue serait détachée, et l'article qui
        // la référence la ferait passer à l'état « Added » — donc réinsérée, sur une ligne
        // qui existe déjà. Le visiteur verrait « slug déjà pris » au lieu de son article.
        return await unitOfWork.Repository<Rubrique>().TrackedQuery
            .FirstOrDefaultAsync(rubrique => rubrique.Id == rubriqueId, cancellationToken)
        ?? throw new DomainException("not_found", "Cette rubrique n'existe pas.");
    }
}
