using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Threading;

namespace DotnetNiger.UI.Services.Mock;

public class MockNewsletterService : INewsletterService
{
    private readonly List<(string Email, DateTime SubscribedAt)> _subscribers = new()
    {
        ("alice@example.com", DateTime.Now.AddDays(-30)),
        ("bob@example.com", DateTime.Now.AddDays(-15)),
        ("charlie@example.com", DateTime.Now.AddDays(-7)),
        ("diana@example.com", DateTime.Now.AddDays(-1)),
    };

    public Task<bool> SubscribeAsync(SubscribeRequest request, CancellationToken cancellationToken = default)
    {
        if (_subscribers.Any(s => s.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
            return Task.FromResult(false);
        _subscribers.Add((request.Email, DateTime.Now));
        return Task.FromResult(true);
    }

    public Task<bool> UnsubscribeAsync(UnsubscribeRequest request, CancellationToken cancellationToken = default)
    {
        var removed = _subscribers.RemoveAll(s =>
            s.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)) > 0;
        return Task.FromResult(removed);
    }

    public Task<bool> DeleteSubscriberAsync(string email, CancellationToken cancellationToken = default)
    {
        var removed = _subscribers.RemoveAll(s =>
            s.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) > 0;
        return Task.FromResult(removed);
    }

    public Task<List<NewsletterSubscriberDto>> GetAllSubscribersAsync()
    {
        var list = _subscribers.Select(s => new NewsletterSubscriberDto
        {
            Email = s.Email,
            SubscribedAt = s.SubscribedAt,
            IsConfirmed = true
        }).ToList();
        return Task.FromResult(list);
    }
}
