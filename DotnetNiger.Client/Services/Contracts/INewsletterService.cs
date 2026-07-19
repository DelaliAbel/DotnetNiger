using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface INewsletterService
{
    Task<bool> SubscribeAsync(SubscribeRequest request);
    Task<bool> UnsubscribeAsync(UnsubscribeRequest request);
    Task<bool> DeleteSubscriberAsync(string email);
    Task<List<NewsletterSubscriberDto>> GetAllSubscribersAsync();
}
