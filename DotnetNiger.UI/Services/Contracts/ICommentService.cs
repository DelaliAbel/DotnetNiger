using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface ICommentService
{
    Task<Guid> GetCurrentUserIdAsync(CancellationToken cancellationToken = default);
    Task<List<CommentResponse>> GetCommentsByPostIdAsync(Guid postId);
    Task<List<CommentResponse>> GetCommentsByEventIdAsync(Guid eventId);
    Task<CommentResponse?> GetCommentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CommentResponse?> CreateCommentAsync(CreateCommentRequest request, CancellationToken cancellationToken = default);
    Task<CommentResponse?> UpdateCommentAsync(UpdateCommentRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCommentAsync(DeleteCommentRequest request, CancellationToken cancellationToken = default);
    Task<List<CommentResponse>> GetAllCommentsAsync();
    Task<bool> ReportCommentAsync(Guid id, CancellationToken cancellationToken = default);
}
