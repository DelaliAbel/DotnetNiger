using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Repositories;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;

    public CommentService(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
    {
        return await _commentRepository.GetAllAsync();
    }

    public async Task<Comment?> GetCommentByIdAsync(Guid id)
    {
        return await _commentRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId)
    {
        return await _commentRepository.GetByPostIdAsync(postId);
    }

    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        comment.Id = Guid.NewGuid();
        comment.CreatedAt = DateTime.UtcNow;
        comment.IsApproved = true;
        return await _commentRepository.AddAsync(comment);
    }

    public async Task<Comment> UpdateCommentAsync(Comment comment)
    {
        comment.UpdatedAt = DateTime.UtcNow;
        return await _commentRepository.UpdateAsync(comment);
    }

    public async Task<bool> DeleteCommentAsync(Guid id)
    {
        return await _commentRepository.DeleteAsync(id);
    }
}
