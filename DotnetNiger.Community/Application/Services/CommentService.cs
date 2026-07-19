using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des commentaires avec arborescence (réponses imbriquées).</summary>
public class CommentService(AppDbContext db) : ICommentService
{
    /// <summary>Commentaires d'un article, construits en arbre à partir de la liste plate.</summary>
    public async Task<List<CommentResponse>> GetByPostIdAsync(Guid postId)
    {
        var comments = await db.Comments.AsNoTracking()
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return BuildTree(comments);
    }

    /// <summary>Commentaires d'un événement, construits en arbre.</summary>
    public async Task<List<CommentResponse>> GetByEventIdAsync(Guid eventId)
    {
        var comments = await db.Comments.AsNoTracking()
            .Where(c => c.EventId == eventId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return BuildTree(comments);
    }

    /// <summary>Liste tous les commentaires pour l'administration (modération).</summary>
    public async Task<List<CommentResponse>> GetAllAsync()
    {
        var comments = await db.Comments.AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return comments.Select(MapComment).ToList();
    }

    /// <summary>Détail d'un commentaire.</summary>
    public async Task<CommentResponse?> GetByIdAsync(Guid id)
    {
        var comment = await db.Comments.FindAsync(id);
        return comment is null ? null : MapComment(comment);
    }

    /// <summary>Ajoute un commentaire (réponse à un article, un événement ou un autre commentaire).</summary>
    public async Task<CommentResponse> CreateAsync(CreateCommentRequest request, Guid userId, string authorName, string authorAvatar)
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            UserId = userId,
            AuthorName = authorName,
            AuthorAvatar = authorAvatar,
            PostId = request.PostId,
            EventId = request.EventId,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        db.Comments.Add(comment);
        await db.SaveChangesAsync();
        return MapComment(comment);
    }

    /// <summary>Modifie le contenu d'un commentaire (refusé si l'utilisateur n'est ni le propriétaire ni admin).</summary>
    public async Task<CommentResponse?> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId, bool isAdmin = false)
    {
        var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null || (comment.UserId != userId && !isAdmin)) return null;

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return MapComment(comment);
    }

    /// <summary>
    /// Supprime un commentaire. S'il a des réponses, son contenu est masqué plutôt que supprimé,
    /// sauf si <paramref name="deleteAllReplies"/> est vrai (supprime aussi les réponses).
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin = false, bool deleteAllReplies = false)
    {
        var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null || (comment.UserId != userId && !isAdmin)) return false;

        if (deleteAllReplies)
        {
            var replies = await db.Comments.Where(c => c.ParentCommentId == id).ToListAsync();
            db.Comments.RemoveRange(replies);
        }
        else
        {
            var hasReplies = await db.Comments.AnyAsync(c => c.ParentCommentId == id);
            if (hasReplies)
            {
                comment.Content = "[Supprimé]";
                comment.UserId = Guid.Empty;
                await db.SaveChangesAsync();
                return true;
            }
        }

        db.Comments.Remove(comment);
        await db.SaveChangesAsync();
        return true;
    }

    private static List<CommentResponse> BuildTree(List<Comment> comments)
    {
        var map = comments.DistinctBy(c => c.Id).Select(MapComment).ToDictionary(c => c.Id);
        var roots = new List<CommentResponse>();

        foreach (var c in map.Values)
        {
            if (c.ParentCommentId is null || !map.ContainsKey(c.ParentCommentId.Value))
                roots.Add(c);
            else if (map.TryGetValue(c.ParentCommentId.Value, out var parent))
                parent.Replies.Add(c);
        }

        return roots;
    }

    private static CommentResponse MapComment(Comment c) => new()
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
        UpdatedAt = c.UpdatedAt
    };
}
