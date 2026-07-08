using System.Security.Cryptography;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des abonnements à la newsletter avec tokens de désabonnement sécurisés.</summary>
public class NewsletterService(AppDbContext db) : INewsletterService
{
    /// <summary>Inscrit un email. S'il était déjà abonné et inactif, le réactive.</summary>
    public async Task<NewsletterSubscriptionResponse> SubscribeAsync(SubscribeRequest request)
    {
        var existing = await db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == request.Email);

        if (existing != null)
        {
            if (existing.IsActive)
                throw new InvalidOperationException(Messages.Newsletter.AlreadySubscribed);

            existing.IsActive = true;
            existing.Name = request.Name;
            existing.UnsubscribedAt = null;
            existing.UnsubscribeToken = GenerateToken();
            await db.SaveChangesAsync();
            return MapSubscription(existing);
        }

        var sub = new NewsletterSubscription
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Name,
            IsActive = true,
            UnsubscribeToken = GenerateToken(),
            SubscribedAt = DateTime.UtcNow
        };

        db.Add(sub);
        await db.SaveChangesAsync();
        return MapSubscription(sub);
    }

    /// <summary>Désabonne via email + token. Le token est généré à l'inscription pour éviter les désabonnements abusifs.</summary>
    public async Task<bool> UnsubscribeAsync(UnsubscribeRequest request)
    {
        var sub = await db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == request.Email && s.UnsubscribeToken == request.Token);

        if (sub is null || !sub.IsActive) return false;

        sub.IsActive = false;
        sub.UnsubscribedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Supprime un abonné par email (réservé admin).</summary>
    public async Task<bool> DeleteByEmailAsync(string email)
    {
        var sub = await db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == email);

        if (sub is null) return false;

        db.Remove(sub);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Liste paginée des abonnés, du plus récent au plus ancien.</summary>
    public async Task<PaginatedResponse<NewsletterSubscriptionResponse>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var query = db.Set<NewsletterSubscription>().AsNoTracking().OrderByDescending(s => s.SubscribedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => MapSubscription(s))
            .ToListAsync();

        return new PaginatedResponse<NewsletterSubscriptionResponse>
            { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    /// <summary>Nombre d'abonnés actuellement actifs.</summary>
    public async Task<int> GetActiveCountAsync()
    {
        return await db.Set<NewsletterSubscription>().AsNoTracking().CountAsync(s => s.IsActive);
    }

    private static NewsletterSubscriptionResponse MapSubscription(NewsletterSubscription s) => new(
        s.Id, s.Email, s.Name, s.IsActive, s.SubscribedAt);

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
