using System.Linq.Expressions;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Infrastructure.Repositories;

/// <summary>
/// Interface générique pour les opérations CRUD
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    // Read
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    // Create
    Task<TEntity> AddAsync(TEntity entity);
    Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);

    // Update
    Task<TEntity> UpdateAsync(TEntity entity);

    // Delete
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAsync(TEntity entity);
    Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate);

    // Count
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

    // Exists
    Task<bool> ExistsAsync(Guid id);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
}
