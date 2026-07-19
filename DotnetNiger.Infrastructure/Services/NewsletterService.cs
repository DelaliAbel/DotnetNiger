using Microsoft.EntityFrameworkCore;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public class NewsletterService : INewsletterService
{
    private readonly DotnetNigerDbContext _db;

    public NewsletterService(DotnetNigerDbContext db) => _db = db;

    public async Task<NewsletterSubscriptionResponse> SubscribeAsync(SubscribeRequest request)
    {
        var existing = await _db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == request.Email);

        if (existing != null)
        {
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.UnsubscribedAt = null;
                await _db.SaveChangesAsync();
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
        await _db.SaveChangesAsync();
        return MapToResponse(sub);
    }

    public async Task<bool> UnsubscribeAsync(UnsubscribeRequest request)
    {
        var sub = await _db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == request.Email && s.UnsubscribeToken == request.Token);
        if (sub == null) return false;
        sub.IsActive = false;
        sub.UnsubscribedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByEmailAsync(string email)
    {
        var sub = await _db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == email);
        if (sub == null) return false;
        _db.Set<NewsletterSubscription>().Remove(sub);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PaginatedResponse<NewsletterSubscriptionResponse>> GetAllAsync(int page, int pageSize)
    {
        var q = _db.Set<NewsletterSubscription>().AsNoTracking();
        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(s => s.SubscribedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PaginatedResponse<NewsletterSubscriptionResponse>(
            items.Select(MapToResponse).ToList(), total, page, pageSize);
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _db.Set<NewsletterSubscription>().CountAsync(s => s.IsActive);
    }

    private static NewsletterSubscriptionResponse MapToResponse(NewsletterSubscription s) =>
        new(s.Id, s.Email, s.Name, s.IsActive, s.SubscribedAt);
}
