using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface INewsletterSubscriptionPersistence
{
    Task<NewsletterSubscription?> GetByEmailAsync(string email);
    Task<NewsletterSubscription?> GetByVerificationTokenAsync(string token);
    Task<NewsletterSubscription?> GetByIdAsync(Guid id);
    Task<List<NewsletterSubscription>> GetSubscriptionsAsync(bool? isActive = null, bool? isVerified = null);
    Task<List<NewsletterSubscription>> GetActiveSubscriptionsAsync();
    Task<List<NewsletterSubscription>> GetUnverifiedSubscriptionsAsync();
    Task AddAsync(NewsletterSubscription subscription);
    Task UpdateAsync(NewsletterSubscription subscription);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string email);
}
