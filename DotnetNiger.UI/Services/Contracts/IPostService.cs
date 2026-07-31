using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface IPostService
{
    Task<List<PostDto>> GetPublishedPostsAsync();
    Task<List<PostDto>> GetPostsByCategoryAsync(string categorySlug);
    Task<List<PostDto>> GetPostsByTagAsync(string tagSlug);
    Task<List<PostDto>> GetAllPostsAsync();
    Task<PostDto?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<PostDto?> GetPostBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<PostDto>> SearchPostsAsync(string query);
    Task<PostDto?> CreatePostAsync(CreatePostRequest request,Guid UserId, CancellationToken cancellationToken = default);
    Task<PostDto?> UpdatePostAsync(Guid postId, UpdatePostRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeletePostAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<bool> PublishPostAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<bool> UnPublishPostAsync(Guid postId, CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<PostDto>> GetAdminPostsAsync(string? status = null);
    Task<List<PostDto>> GetMyPostsAsync();
}
