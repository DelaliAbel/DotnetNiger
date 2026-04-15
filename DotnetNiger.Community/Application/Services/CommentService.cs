using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Domain.Entities;
using System.Web;
using DotnetNiger.Community.Application.Constants;

namespace DotnetNiger.Community.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentPersistence _commentRepository;

    public CommentService(ICommentPersistence commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<Comment>> GetAllCommentsAsync(int page = ValidationConstants.DefaultPage, int pageSize = 20)
    {
        // Server-side pagination: Use database-side Skip/Take
        page = Math.Max(1, page);
        pageSize = Math.Min(pageSize, ValidationConstants.MaxPageSize); // Cap at 100 for safety
        return await _commentRepository.GetPagedAsync(page, pageSize);
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
        // XSS Protection: Encode HTML content to prevent script injection
        comment.Content = HttpUtility.HtmlEncode(comment.Content);
        comment.Id = Guid.NewGuid();
        comment.CreatedAt = DateTime.UtcNow;
        comment.IsApproved = true;
        return await _commentRepository.AddAsync(comment);
    }

    public async Task<Comment> UpdateCommentAsync(Comment comment)
    {
        // XSS Protection: Encode HTML content to prevent script injection
        comment.Content = HttpUtility.HtmlEncode(comment.Content);
        comment.UpdatedAt = DateTime.UtcNow;
        return await _commentRepository.UpdateAsync(comment);
    }

    public async Task<bool> DeleteCommentAsync(Guid id)
    {
        return await _commentRepository.DeleteAsync(id);
    }
}
