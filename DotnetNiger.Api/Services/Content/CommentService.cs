using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Service de gestion des commentaires sur les articles et événements.</summary>
public class CommentService : ICommentService
{
    private readonly DotnetNigerDbContext _db;

    public CommentService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère les commentaires racines d'un article.</summary>
    public async Task<List<CommentResponse>> GetByPostIdAsync(Guid postId)
    {
        var comments = await _db.Comments.AsNoTracking()
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
            .Where(c => c.PostId == postId && c.ParentCommentId == null && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return comments.Select(MapToResponse).ToList();
    }

    /// <summary>Récupère les commentaires racines d'un événement.</summary>
    public async Task<List<CommentResponse>> GetByEventIdAsync(Guid eventId)
    {
        var comments = await _db.Comments.AsNoTracking()
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
            .Where(c => c.EventId == eventId && c.ParentCommentId == null && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return comments.Select(MapToResponse).ToList();
    }

    /// <summary>Récupère un commentaire par identifiant.</summary>
    public async Task<CommentResponse?> GetByIdAsync(Guid id)
    {
        var comment = await _db.Comments
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        return comment == null ? null : MapToResponse(comment);
    }

    /// <summary>Crée un commentaire sur un article ou un événement.</summary>
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

    /// <summary>Met à jour le contenu d'un commentaire (auteur uniquement).</summary>
    public async Task<CommentResponse?> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted);
        if (comment == null) return null;
        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(comment);
    }

    /// <summary>Supprime un commentaire (suppression logique).</summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin, bool deleteAllReplies)
    {
        var comment = await _db.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (comment == null) return false;
        if (!isAdmin && comment.UserId != userId)
            throw new UnauthorizedAccessException("Vous ne pouvez supprimer que vos propres commentaires.");

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;

        if (deleteAllReplies && comment.Replies != null && comment.Replies.Count != 0)
        {
            foreach (var reply in comment.Replies.Where(r => !r.IsDeleted))
            {
                reply.IsDeleted = true;
                reply.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Récupère tous les commentaires de la plateforme.</summary>
    public async Task<List<CommentResponse>> GetAllAsync()
    {
        var comments = await _db.Comments.AsNoTracking()
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
            .Where(c => !c.IsDeleted)
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
            response.Replies = c.Replies.Where(r => !r.IsDeleted).Select(MapToResponse).ToList();

        return response;
    }
}
