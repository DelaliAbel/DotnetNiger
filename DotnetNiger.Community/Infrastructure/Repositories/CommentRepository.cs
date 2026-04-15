using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface ICommentRepository : IRepository<Comment>, ICommentPersistence
{
}

public class CommentRepository : BaseRepository<Comment>, ICommentRepository
{
    public CommentRepository(CommunityDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId)
    {
        return await _dbSet
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}
