using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface IPostService
{
    Task<List<PostDto>> GetPublishedPostsAsync();
    Task<List<PostDto>> GetPostsByCategoryAsync(string categorySlug);
    Task<List<PostDto>> GetPostsByTagAsync(string tagSlug);
    Task<List<PostDto>> GetAllPostsAsync();
    Task<PostDto?> GetPostByIdAsync(Guid postId);
    Task<PostDto?> GetPostBySlugAsync(string slug);
    Task<List<PostDto>> SearchPostsAsync(string query);
    Task<PostDto?> CreatePostAsync(CreatePostRequest request,Guid UserId);
    Task<PostDto?> UpdatePostAsync(Guid postId, UpdatePostRequest request);
    Task<bool> DeletePostAsync(Guid postId);
    Task<bool> PublishPostAsync(Guid postId);
    Task<bool> UnPublishPostAsync(Guid postId);
    Task IncrementViewCountAsync(Guid id);
    Task<List<PostDto>> GetAdminPostsAsync(string? status = null);
    Task<List<PostDto>> GetMyPostsAsync();
}
