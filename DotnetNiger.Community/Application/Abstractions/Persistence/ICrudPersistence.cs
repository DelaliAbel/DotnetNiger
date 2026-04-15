using System.Linq.Expressions;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface ICrudPersistence<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetPagedAsync(int page = 1, int pageSize = 10);
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity> AddAsync(TEntity entity);
    Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAsync(TEntity entity);
    Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
    Task<IEnumerable<TEntity>> SearchAsync(Expression<Func<TEntity, bool>> predicate, int page = 1, int pageSize = 20);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
}
