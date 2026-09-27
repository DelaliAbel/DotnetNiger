using System.Linq.Expressions;
using DotnetNiger.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Persistence;

/// <summary>
/// Materialisation d'une page de résultats à partir d'une requête.
/// </summary>
/// <remarks>
/// Placée dans Infrastructure et non dans Application : <c>CountAsync</c> et
/// <c>ToListAsync</c> viennent d'Entity Framework, et la couche application n'a pas à
/// dépendre d'un ORM.
/// </remarks>
public static class PagingExtensions
{
    /// <summary>
    /// Exécute la requête et renvoie une page, avec le nombre total d'éléments correspondant au filtre.
    /// </summary>
    /// <typeparam name="TEntity">Type des entités.</typeparam>
    /// <param name="query">Requête filtrée et triée par l'appelant.</param>
    /// <param name="page">Page demandée.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résultats.</returns>
    public static async Task<PagedResult<TEntity>> ToPagedResultAsync<TEntity>(
        this IQueryable<TEntity> query,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<TEntity>(items, page.PageNumber, page.PageSize, totalCount);
    }

    /// <summary>
    /// Projette puis pagine : seules les colonnes utiles sont lues depuis la base.
    /// </summary>
    /// <typeparam name="TSource">Type de l'entité.</typeparam>
    /// <typeparam name="TResult">Type du DTO renvoyé.</typeparam>
    /// <param name="query">Requête filtrée et triée par l'appelant.</param>
    /// <param name="page">Page demandée.</param>
    /// <param name="selector">Projection vers le DTO.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de DTO.</returns>
    public static async Task<PagedResult<TResult>> ToPagedResultAsync<TSource, TResult>(
        this IQueryable<TSource> query,
        PageRequest page,
        Expression<Func<TSource, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(selector);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(selector)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<TResult>(items, page.PageNumber, page.PageSize, totalCount);
    }
}
