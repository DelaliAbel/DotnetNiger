using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface IPostService
{
    Task<IEnumerable<Post>> GetAllPublishedPostsAsync(int page = 1, int pageSize = 10);
    Task<Post?> GetPostByIdAsync(Guid id);
    Task<Post?> GetPostBySlugAsync(string slug);
    Task<Post> CreatePostAsync(Post post);
    Task<Post> UpdatePostAsync(Post post);
    Task<bool> DeletePostAsync(Guid id);
}