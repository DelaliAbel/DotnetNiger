using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Community.Infrastructure.Data;
using DotnetNiger.Community.Domain.Interfaces;

namespace DotnetNiger.Community.Infrastructure.Repositories;

/// <summary>
/// Implémentation générique du repository
/// </summary>
public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly CommunityDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    private static readonly string? DefaultOrderProperty = ResolveDefaultOrderProperty();

    public BaseRepository(CommunityDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetPagedAsync(int page = 1, int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Min(Math.Max(1, pageSize), 100); // Clamp pageSize to [1..100]
        return await ApplyDefaultOrdering(_dbSet.AsNoTracking())
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Get paginated results with predicate and ordering (SQL-optimized)
    /// Returns Items AND Total count for client-side pagination
    /// </summary>
    public async Task<(IEnumerable<TEntity> Items, int Total)> GetPagedWithCountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        int page = 1,
        int pageSize = 10,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Min(Math.Max(1, pageSize), 100);

        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (predicate != null)
            query = query.Where(predicate);

        // Get total count BEFORE ordering (more efficient)
        var total = await query.CountAsync();

        if (orderBy != null)
            query = orderBy(query);
        else
            query = ApplyDefaultOrdering(query);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    /// <summary>
    /// Search with LIKE on predicate (SQL-optimized with pagination)
    /// </summary>
    public async Task<IEnumerable<TEntity>> SearchAsync(
        Expression<Func<TEntity, bool>> predicate,
        int page = 1,
        int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Min(Math.Max(1, pageSize), 100);

        return await ApplyDefaultOrdering(_dbSet.Where(predicate))
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    private static IQueryable<TEntity> ApplyDefaultOrdering(IQueryable<TEntity> query)
    {
        return DefaultOrderProperty == null
            ? query
            : query.OrderByDescending(e => EF.Property<object>(e, DefaultOrderProperty));
    }

    private static string? ResolveDefaultOrderProperty()
    {
        var type = typeof(TEntity);
        if (type.GetProperty("CreatedAt") != null)
            return "CreatedAt";
        if (type.GetProperty("JoinedAt") != null)
            return "JoinedAt";
        if (type.GetProperty("Id") != null)
            return "Id";

        return null;
    }

    public async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
    {
        _dbSet.AddRange(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(TEntity entity)
    {
        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync();
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
        return entities.Count;
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.FindAsync(id) != null;
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }
}
