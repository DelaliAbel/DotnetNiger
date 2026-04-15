using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface INewsletterSubscriptionRepository : INewsletterSubscriptionPersistence
{
}

public class NewsletterSubscriptionRepository : INewsletterSubscriptionRepository
{
    private readonly CommunityDbContext _context;

    public NewsletterSubscriptionRepository(CommunityDbContext context)
    {
        _context = context;
    }

    public async Task<NewsletterSubscription?> GetByEmailAsync(string email)
    {
        return await _context.NewsletterSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(ns => ns.Email == email.ToLower());
    }

    public async Task<NewsletterSubscription?> GetByVerificationTokenAsync(string token)
    {
        return await _context.NewsletterSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(ns => ns.VerificationToken == token);
    }

    public async Task<NewsletterSubscription?> GetByIdAsync(Guid id)
    {
        return await _context.NewsletterSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(ns => ns.Id == id);
    }

    public async Task<List<NewsletterSubscription>> GetSubscriptionsAsync(bool? isActive = null, bool? isVerified = null)
    {
        var query = _context.NewsletterSubscriptions.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(ns => ns.IsActive == isActive.Value);
        }

        if (isVerified.HasValue)
        {
            query = query.Where(ns => ns.IsVerified == isVerified.Value);
        }

        return await query
            .OrderByDescending(ns => ns.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<NewsletterSubscription>> GetActiveSubscriptionsAsync()
    {
        return await _context.NewsletterSubscriptions
            .AsNoTracking()
            .Where(ns => ns.IsActive && ns.IsVerified)
            .ToListAsync();
    }

    public async Task<List<NewsletterSubscription>> GetUnverifiedSubscriptionsAsync()
    {
        return await _context.NewsletterSubscriptions
            .AsNoTracking()
            .Where(ns => !ns.IsVerified && ns.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(NewsletterSubscription subscription)
    {
        await _context.NewsletterSubscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(NewsletterSubscription subscription)
    {
        _context.NewsletterSubscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var subscription = await GetByIdAsync(id);
        if (subscription != null)
        {
            _context.NewsletterSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.NewsletterSubscriptions
            .AnyAsync(ns => ns.Email == email.ToLower());
    }
}
