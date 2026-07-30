using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface INewsletterService
{
    Task<bool> SubscribeAsync(SubscribeRequest request, CancellationToken cancellationToken = default);
    Task<bool> UnsubscribeAsync(UnsubscribeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteSubscriberAsync(string email, CancellationToken cancellationToken = default);
    Task<List<NewsletterSubscriberDto>> GetAllSubscribersAsync();
}
