using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<Comment>> GetAllCommentsAsync(int page = 1, int pageSize = 20);
    Task<Comment?> GetCommentByIdAsync(Guid id);
    Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId);
    Task<Comment> CreateCommentAsync(Comment comment);
    Task<Comment> UpdateCommentAsync(Comment comment);
    Task<bool> DeleteCommentAsync(Guid id);
}
