using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetPublishedPostsAsync(int page = 1, int pageSize = 10);
    Task<Post?> GetBySlugAsync(string slug);
    Task<IEnumerable<Post>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10);
    Task<IEnumerable<Post>> GetByTagAsync(Guid tagId, int page = 1, int pageSize = 10);
}
