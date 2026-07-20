using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.Content;

public interface ICommentService
{
    Task<List<CommentResponse>> GetByPostIdAsync(Guid postId);
    Task<List<CommentResponse>> GetByEventIdAsync(Guid eventId);
    Task<CommentResponse?> GetByIdAsync(Guid id);
    Task<CommentResponse> CreateAsync(CreateCommentRequest request, Guid userId, string userName, string? avatar);
    Task<CommentResponse?> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId, bool deleteAllReplies);
    Task<List<CommentResponse>> GetAllAsync();
}
