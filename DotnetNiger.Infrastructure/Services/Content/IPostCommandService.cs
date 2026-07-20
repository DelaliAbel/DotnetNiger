using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.Content;

public interface IPostCommandService
{
    Task<PostResponse> CreateAsync(CreatePostRequest request, Guid authorId, string authorName, bool isAdmin, bool isCollaborator);
    Task<PostResponse?> UpdateAsync(Guid id, UpdatePostRequest request, Guid userId, bool isAdmin);
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    Task<PostResponse?> IncrementViewCountAsync(Guid id);
    Task SubmitForReviewAsync(Guid id);
    Task PublishAsync(Guid id);
    Task ArchiveAsync(Guid id);
}
