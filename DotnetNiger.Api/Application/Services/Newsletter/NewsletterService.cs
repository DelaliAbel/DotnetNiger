using System.Threading;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Newsletter;

/// <summary>Service de gestion des inscriptions à la newsletter.</summary>
public class NewsletterService : INewsletterService
{
    private readonly DotnetNigerDbContext _db;

    public NewsletterService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Inscrit un email à la newsletter (réactive si désinscrit).</summary>
    public async Task<NewsletterSubscriptionResponse> SubscribeAsync(SubscribeRequest request, CancellationToken ct = default)
    {
        var existing = await _db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == request.Email, ct);

        if (existing != null)
        {
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.UnsubscribedAt = null;
                await _db.SaveChangesAsync(ct);
            }
            return MapToResponse(existing);
        }

        var sub = new NewsletterSubscription
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Name,
            UnsubscribeToken = Guid.NewGuid().ToString("N"),
            IsActive = true,
            SubscribedAt = DateTime.UtcNow
        };
        _db.Set<NewsletterSubscription>().Add(sub);
        await _db.SaveChangesAsync(ct);
        return MapToResponse(sub);
    }

    /// <summary>Désinscrit un email de la newsletter via un token.</summary>
    public async Task<bool> UnsubscribeAsync(UnsubscribeRequest request, CancellationToken ct = default)
    {
        var sub = await _db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == request.Email && s.UnsubscribeToken == request.Token, ct);
        if (sub == null) return false;
        sub.IsActive = false;
        sub.UnsubscribedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Supprime définitivement une inscription par email.</summary>
    public async Task<bool> DeleteByEmailAsync(string email, CancellationToken ct = default)
    {
        var sub = await _db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == email, ct);
        if (sub == null) return false;
        _db.Set<NewsletterSubscription>().Remove(sub);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Récupère la liste paginée des inscriptions.</summary>
    public async Task<PaginatedResponse<NewsletterSubscriptionResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.Set<NewsletterSubscription>().AsNoTracking();
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(s => s.SubscribedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new PaginatedResponse<NewsletterSubscriptionResponse>(
            items.Select(MapToResponse).ToList(), total, page, pageSize);
    }

    /// <summary>Retourne le nombre d'inscriptions actives.</summary>
    public async Task<int> GetActiveCountAsync(CancellationToken ct = default)
    {
        return await _db.Set<NewsletterSubscription>().CountAsync(s => s.IsActive, ct);
    }

    private static NewsletterSubscriptionResponse MapToResponse(NewsletterSubscription s) =>
        new(s.Id, s.Email, s.Name, s.IsActive, s.SubscribedAt);
}
