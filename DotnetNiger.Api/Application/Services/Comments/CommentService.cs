using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Comments;

/// <summary>Service de gestion des commentaires sur les articles et événements.</summary>
public class CommentService : ICommentService
{
    private readonly DotnetNigerDbContext _db;

    public CommentService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère les commentaires racines d'un article publié.</summary>
    public async Task<List<CommentResponse>> GetByPostIdAsync(Guid postId)
    {
        var postPublished = await _db.Posts.AsNoTracking()
            .AnyAsync(p => p.Id == postId && p.Status == PostStatus.Published);
        if (!postPublished) return [];

        var comments = await _db.Comments.AsNoTracking()
            .Include(c => c.Replies)
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return comments.Select(MapToResponse).ToList();
    }

    /// <summary>Récupère les commentaires racines d'un événement publié.</summary>
    public async Task<List<CommentResponse>> GetByEventIdAsync(Guid eventId)
    {
        var eventPublished = await _db.Events.AsNoTracking()
            .AnyAsync(e => e.Id == eventId && e.Status == EventStatus.Published);
        if (!eventPublished) return [];

        var comments = await _db.Comments.AsNoTracking()
            .Include(c => c.Replies)
            .Where(c => c.EventId == eventId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return comments.Select(MapToResponse).ToList();
    }

    /// <summary>Récupère un commentaire par identifiant.</summary>
    public async Task<CommentResponse?> GetByIdAsync(Guid id)
    {
        var comment = await _db.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (comment == null) return null;

        var published = (comment.PostId.HasValue
                && await _db.Posts.AsNoTracking().AnyAsync(p => p.Id == comment.PostId && p.Status == PostStatus.Published))
            || (comment.EventId.HasValue
                && await _db.Events.AsNoTracking().AnyAsync(e => e.Id == comment.EventId && e.Status == EventStatus.Published));
        return published ? MapToResponse(comment) : null;
    }

    /// <summary>Crée un commentaire sur un article ou un événement publié.</summary>
    public async Task<CommentResponse> CreateAsync(CreateCommentRequest request, Guid userId, string userName, string? avatar)
    {
        if (request.PostId.HasValue == request.EventId.HasValue)
            throw new InvalidOperationException("Le commentaire doit être lié à un article OU à un événement.");

        if (request.PostId.HasValue)
        {
            var postPublished = await _db.Posts.AsNoTracking()
                .AnyAsync(p => p.Id == request.PostId && p.Status == PostStatus.Published);
            if (!postPublished)
                throw new InvalidOperationException("Impossible de commenter un article non publié.");
        }
        else
        {
            var eventPublished = await _db.Events.AsNoTracking()
                .AnyAsync(e => e.Id == request.EventId && e.Status == EventStatus.Published);
            if (!eventPublished)
                throw new InvalidOperationException("Impossible de commenter un événement non publié.");
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            UserId = userId,
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
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (comment == null) return null;
        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(comment);
    }

    /// <summary>Supprime un commentaire et sa filiale de réponses (suppression définitive).</summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin, bool deleteAllReplies)
    {
        var comment = await _db.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (comment == null) return false;
        if (!isAdmin && comment.UserId != userId)
            throw new UnauthorizedAccessException("Vous ne pouvez supprimer que vos propres commentaires.");

        if (comment.Replies != null && comment.Replies.Count != 0)
        {
            if (!isAdmin && !deleteAllReplies)
                throw new UnauthorizedAccessException("Ce commentaire possède des réponses. Supprimez d'abord les réponses ou choisissez la suppression de la discussion.");
            _db.Comments.RemoveRange(comment.Replies);
        }

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Récupère tous les commentaires de la plateforme.</summary>
    public async Task<List<CommentResponse>> GetAllAsync()
    {
        var comments = await _db.Comments.AsNoTracking()
            .Include(c => c.Replies)
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
