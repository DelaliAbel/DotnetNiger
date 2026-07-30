using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Services.Contracts;
using System.Threading;

namespace DotnetNiger.UI.Services.Mock;

public class MockContactService : IContactService
{
    private readonly List<ContactRequest> _messages = new();

    public Task<bool> SendAsync(ContactRequest request, CancellationToken cancellationToken = default)
    {
        _messages.Add(request);
        return Task.FromResult(true);
    }
}
