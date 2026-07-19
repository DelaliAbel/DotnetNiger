using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services;

public interface INewsletterService
{
    Task<NewsletterSubscriptionResponse> SubscribeAsync(SubscribeRequest request);
    Task<bool> UnsubscribeAsync(UnsubscribeRequest request);
    Task<bool> DeleteByEmailAsync(string email);
    Task<PaginatedResponse<NewsletterSubscriptionResponse>> GetAllAsync(int page, int pageSize);
    Task<int> GetActiveCountAsync();
}
