using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.General;

public interface INewsletterService
{
    Task<NewsletterSubscriptionResponse> SubscribeAsync(SubscribeRequest request);
    Task<bool> UnsubscribeAsync(UnsubscribeRequest request);
    Task<bool> DeleteByEmailAsync(string email);
    Task<PaginatedResponse<NewsletterSubscriptionResponse>> GetAllAsync(int page, int pageSize);
    Task<int> GetActiveCountAsync();
}
