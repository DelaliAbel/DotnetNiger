using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implémentation EF Core du dépôt en lecture.
/// </summary>
/// <typeparam name="T">Type de l'entité.</typeparam>
internal sealed class EfReadRepository<T> : IReadRepository<T>
    where T : class, IEntity
{
    /// <summary>
    /// Initialise le dépôt.
    /// </summary>
    /// <param name="context">Contexte de persistance.</param>
    public EfReadRepository(DotnetNigerDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Query = context.Set<T>().AsNoTracking();
    }

    /// <inheritdoc />
    /// <remarks>
    /// <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}"/> est appliqué
    /// par défaut : la couche de lecture ne modifie rien, et le suivi ne servirait qu'à
    /// consommer de la mémoire sur un hébergement limité à 256 Mo.
    /// </remarks>
    public IQueryable<T> Query { get; }
}
