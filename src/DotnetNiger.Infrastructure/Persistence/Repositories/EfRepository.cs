using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implémentation EF Core du dépôt en écriture générique.
/// </summary>
/// <typeparam name="T">Type de l'entité.</typeparam>
internal class EfRepository<T> : IRepository<T>
    where T : class, IEntity
{
    private readonly DbContext _context;
    private readonly DbSet<T> _set;

    /// <summary>
    /// Initialise le dépôt.
    /// </summary>
    /// <param name="context">Contexte de persistance.</param>
    public EfRepository(DbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // FirstOrDefaultAsync et non FindAsync : Find court-circuite le suivi d'entité
        // et n'applique pas le filtre global de suppression logique. Un contenu retiré
        // serait donc renvoyé par erreur.
        return await _set
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => _set.AsNoTracking().AnyAsync(entity => entity.Id == id, cancellationToken);

    /// <inheritdoc />
    public void Add(T entity) => _set.Add(entity);

    /// <inheritdoc />
    public void Update(T entity) => _set.Update(entity);
}
