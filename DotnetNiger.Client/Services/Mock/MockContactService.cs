using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Mock;

public class MockContactService : IContactService
{
    private readonly List<ContactRequest> _messages = new();

    public Task<bool> SendAsync(ContactRequest request)
    {
        _messages.Add(request);
        return Task.FromResult(true);
    }
}
