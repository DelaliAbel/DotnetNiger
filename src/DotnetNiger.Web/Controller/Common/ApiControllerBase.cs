using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller.Common;

/// <summary>
/// Socle commun des contrôleurs de l'API.
/// </summary>
/// <remarks>
/// Regroupe ce qui est identique partout : préfixe de route sans versionnement, limitation de
/// débit, format de réponse, et les projections paginées. Les contrôleurs enfants n'ont ainsi
/// à écrire que ce qui leur est propre.
/// <para>
/// L'API JSON n'exige pas de jeton antiforgery : le cookie d'authentification est en
/// <c>SameSite=Lax</c> (voir <c>IdentityServiceCollectionExtensions</c>), donc un site tiers ne
/// peut pas faire joindre ce cookie à une requête d'écriture, ce qui ferme la forge de requête
/// intersite. Le jeton reste exigé sur les formulaires SSR, qui naviguent réellement.
/// </para>
/// <para>
/// Ce fichier ne porte que la pagination générique et la résolution des noms d'auteur. La
/// pagination par entité et les gardes d'autorisation sont dans les parties voisines.
/// </para>
/// </remarks>
[ApiController]
[Route(ApiRoutes.Base)]
[EnableRateLimiting("api")]
[Produces("application/json")]
[IgnoreAntiforgeryToken]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
public abstract partial class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Exécute une requête paginée et projette son résultat.
    /// </summary>
    /// <typeparam name="T">Type de l'entité.</typeparam>
    /// <typeparam name="TResult">Type de la réponse.</typeparam>
    /// <param name="requete">Requête filtrée et triée.</param>
    /// <param name="page">Demande de pagination.</param>
    /// <param name="projection">Projection appliquée à chaque élément.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de réponses.</returns>
    protected static async Task<ActionResult<PageResponse<TResult>>> PageOfAsync<T, TResult>(
        IQueryable<T> requete,
        PageRequest page,
        Func<T, TResult> projection,
        CancellationToken cancellationToken)
        where T : class
    {
        var total = await requete.CountAsync(cancellationToken);

        if (total == 0)
        {
            return ApiProjections.ToPage(PagedResultFactory.Empty<T>(page), projection);
        }

        var items = await requete
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<T>(items, page.PageNumber, page.PageSize, total),
            projection);
    }

    /// <summary>
    /// Résout des noms d'auteurs en une requête.
    /// </summary>
    /// <param name="unitOfWork">Unité de travail.</param>
    /// <param name="ids">Identifiants d'auteurs.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Correspondance identifiant vers nom complet.</returns>
    protected static async Task<Dictionary<Guid, string>> NomsAsync(
        IUnitOfWork unitOfWork,
        Guid[] ids,
        CancellationToken cancellationToken)
    {
        if (ids.Length == 0)
        {
            return [];
        }

        return await unitOfWork.ReadRepository<ApplicationUser>().Query
            .Where(user => ids.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.FullName, cancellationToken);
    }
}