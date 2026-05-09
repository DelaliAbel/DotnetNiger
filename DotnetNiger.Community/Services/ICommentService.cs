using DotnetNiger.Community.Dtos.Requests;
using DotnetNiger.Community.Dtos.Responses;

namespace DotnetNiger.Community.Services;

public interface ICommentService
{
    Task<List<CommentResponse>> GetByPostIdAsync(Guid postId);
    Task<List<CommentResponse>> GetByEventIdAsync(Guid eventId);
    Task<CommentResponse?> GetByIdAsync(Guid id);
    Task<CommentResponse> CreateAsync(CreateCommentRequest request, Guid userId, string authorName, string authorAvatar);
    Task<CommentResponse?> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId, bool deleteAllReplies = false);
}
