using Microsoft.EntityFrameworkCore;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly DotnetNigerDbContext _db;

    public CommentService(DotnetNigerDbContext db) => _db = db;

    public async Task<List<CommentResponse>> GetByPostIdAsync(Guid postId)
    {
        var comments = await _db.Comments.AsNoTracking()
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return comments.Select(MapToResponse).ToList();
    }

    public async Task<List<CommentResponse>> GetByEventIdAsync(Guid eventId)
    {
        var comments = await _db.Comments.AsNoTracking()
            .Where(c => c.EventId == eventId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return comments.Select(MapToResponse).ToList();
    }

    public async Task<CommentResponse?> GetByIdAsync(Guid id)
    {
        var comment = await _db.Comments.FindAsync(id);
        return comment == null ? null : MapToResponse(comment);
    }

    public async Task<CommentResponse> CreateAsync(CreateCommentRequest request, Guid userId, string userName, string? avatar)
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            UserId = userId,
            AuthorId = userId,
            AuthorName = userName,
            AuthorAvatar = avatar ?? "",
            PostId = request.PostId,
            EventId = request.EventId,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();
        return MapToResponse(comment);
    }

    public async Task<CommentResponse?> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (comment == null) return null;
        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(comment);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool deleteAllReplies)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id && (c.UserId == userId || deleteAllReplies));
        if (comment == null) return false;
        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<CommentResponse>> GetAllAsync()
    {
        var comments = await _db.Comments.AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return comments.Select(MapToResponse).ToList();
    }

    private CommentResponse MapToResponse(Comment c)
    {
        var response = new CommentResponse
        {
            Id = c.Id,
            Content = c.Content,
            UserId = c.UserId,
            AuthorName = c.AuthorName,
            AuthorAvatar = c.AuthorAvatar,
            PostId = c.PostId ?? Guid.Empty,
            EventId = c.EventId ?? Guid.Empty,
            ParentCommentId = c.ParentCommentId,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            Replies = []
        };

        if (c.Replies != null && c.Replies.Count != 0)
            response.Replies = c.Replies.Select(MapToResponse).ToList();

        return response;
    }
}
