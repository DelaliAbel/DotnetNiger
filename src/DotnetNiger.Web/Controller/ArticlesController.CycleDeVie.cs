using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Cycle de vie d'un article : publication, archivage, retour en brouillon, retrait.
/// </summary>
/// <remarks>
/// Ces quatre opérations partagent le même chargement suivi de l'article avec sa rubrique, et
/// la même règle : seul un administrateur peut agir sur la publication d'un article.
/// </remarks>
public sealed partial class ArticlesController
{
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
            throw new DomainException("categorie_requisa", "Choisissez une rubrique.");
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