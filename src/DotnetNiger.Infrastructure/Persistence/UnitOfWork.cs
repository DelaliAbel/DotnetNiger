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
    private IDbContextTransaction? _currentTransaction;

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
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            return;
        }

        _currentTransaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await _currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        await DisposeTransactionAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        await _currentTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        await DisposeTransactionAsync().ConfigureAwait(false);
    }

    private async ValueTask DisposeTransactionAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync().ConfigureAwait(false);
            _currentTransaction = null;
        }
    }
}
