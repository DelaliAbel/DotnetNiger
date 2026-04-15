using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface ICommentPersistence : ICrudPersistence<Comment>
{
    Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId);
}
