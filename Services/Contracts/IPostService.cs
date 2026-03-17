using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;

namespace DotnetNiger.UI.Services.Contracts;

public interface IPostService
{
    Task<List<PostDto>> GetAllPostsAsync();
    Task<List<PostDto>> GetPublishedPostsAsync();
    Task<List<PostDto>> GetPostsByCategoryAsync(string categorySlug);
    Task<List<PostDto>> GetPostsByTagAsync(string tagSlug);
    Task<PostDto?> GetPostByIdAsync(Guid id);
    Task<PostDto?> GetPostBySlugAsync(string slug);
    Task<PostDto> CreatePostAsync(CreatePostRequest request);
    Task<PostDto?> UpdatePostAsync(Guid id, UpdatePostRequest request);
    Task<bool> DeletePostAsync(Guid id);
    Task<List<PostDto>> SearchPostsAsync(string query);
}
