using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Application.Abstractions.Persistence;

public interface IRepositoryPersistence<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id);
    IQueryable<TEntity> Query();
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
}
