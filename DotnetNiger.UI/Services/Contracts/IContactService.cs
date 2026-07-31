using DotnetNiger.UI.Models.Requests;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface IContactService
{
    Task<bool> SendAsync(ContactRequest request, CancellationToken cancellationToken = default);
}
