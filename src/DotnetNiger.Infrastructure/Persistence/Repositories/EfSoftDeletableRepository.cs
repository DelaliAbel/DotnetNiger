using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implémentation EF Core du dépôt d'une entité supprimable logiquement.
/// </summary>
/// <typeparam name="T">Type de l'entité.</typeparam>
internal sealed class EfSoftDeletableRepository<T> : EfRepository<T>, ISoftDeletableRepository<T>
    where T : class, ISoftDeletable
{
    private readonly DbSet<T> _set;

    /// <summary>
    /// Initialise le dépôt.
    /// </summary>
    /// <param name="context">Contexte de persistance.</param>
    public EfSoftDeletableRepository(DotnetNigerDbContext context)
        : base(context)
    {
        _set = context.Set<T>();
    }

    /// <inheritdoc />
    public void Retire(T entity, DateTime deletedAtUtc, Guid? deletedById = null)
    {
        entity.SoftDelete(deletedAtUtc, deletedById);

        // L'entité est normalement suivie : la détection de changements de EF Core suffit.
        // Si elle a été relue sans suivi, il faut la rattacher explicitement, sinon la
        // modification resterait en mémoire.
        if (_set.Entry(entity).State == EntityState.Detached)
        {
            _set.Update(entity);
        }
    }
}
