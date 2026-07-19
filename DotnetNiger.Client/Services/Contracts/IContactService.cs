using DotnetNiger.Client.Models.Requests;

namespace DotnetNiger.Client.Services.Contracts;

public interface IContactService
{
    Task<bool> SendAsync(ContactRequest request);
}
