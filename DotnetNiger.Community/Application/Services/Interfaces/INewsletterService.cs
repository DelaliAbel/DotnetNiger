using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.DTOs.Requests;
namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface INewsletterService
{
    Task<NewsletterSubscriptionResponse?> SubscribeAsync(string email);
    Task<bool> VerifySubscriptionAsync(string token);
    Task<bool> UnsubscribeAsync(string email, string token);
    Task<RegisterAndSubscribeResponse?> RegisterAndSubscribeAsync(RegisterAndSubscribeRequest request);
    Task<List<NewsletterSubscriptionResponse>> GetActiveSubscriptionsAsync();
    Task<List<NewsletterSubscriptionResponse>> GetSubscriptionsAsync(bool? isActive = null, bool? isVerified = null);
    Task<bool> SetSubscriptionStatusAsync(Guid subscriptionId, bool isActive, bool? isVerified = null);
    Task<bool> DeleteSubscriptionAsync(Guid subscriptionId);
}
