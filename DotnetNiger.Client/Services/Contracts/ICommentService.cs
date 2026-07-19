using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface ICommentService
{
    Task<Guid> GetCurrentUserIdAsync();
    Task<List<CommentResponse>> GetCommentsByPostIdAsync(Guid postId);
    Task<List<CommentResponse>> GetCommentsByEventIdAsync(Guid eventId);
    Task<CommentResponse?> GetCommentByIdAsync(Guid id);
    Task<CommentResponse?> CreateCommentAsync(CreateCommentRequest request);
    Task<CommentResponse?> UpdateCommentAsync(UpdateCommentRequest request);
    Task<bool> DeleteCommentAsync(DeleteCommentRequest request);
    Task<List<CommentResponse>> GetAllCommentsAsync();
}
