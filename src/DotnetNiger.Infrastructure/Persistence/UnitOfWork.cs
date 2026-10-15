using System.Data;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Common;
using DotnetNiger.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DotnetNiger.Infrastructure.Persistence;

/// <summary>
/// Unité de travail adossée au contexte de persistance.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly DotnetNigerDbContext _context;

    /// <summary>
    /// Initialise l'unité de travail.
    /// </summary>
    /// <param name="context">Contexte de persistance.</param>
    public UnitOfWork(DotnetNigerDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public IRepository<T> Repository<T>()
        where T : class, IEntity
        => new EfRepository<T>(_context);

    /// <inheritdoc />
    public ISoftDeletableRepository<T> SoftDeletableRepository<T>()
        where T : class, ISoftDeletable
        => new EfSoftDeletableRepository<T>(_context);

    /// <inheritdoc />
    public IReadRepository<T> ReadRepository<T>()
        where T : class, IEntity
        => new EfReadRepository<T>(_context);

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // CreateExecutionStrategy est le point d'entrée imposé par EF quand la reprise est
        // activée : une transaction ouverte en dehors lève une exception à la première
        // commande. C'est ici que la reprise s'applique, et nowhere ailleurs.
        var strategie = _context.Database.CreateExecutionStrategy();

        await strategie.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)
                .ConfigureAwait(false);

            try
            {
                await operation(cancellationToken).ConfigureAwait(false);

                await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // L'annulation explicite est indispensable : sans elle, une reprise
                // rejouerait le travail par-dessus une transaction encore ouverte.
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }).ConfigureAwait(false);
    }
}
