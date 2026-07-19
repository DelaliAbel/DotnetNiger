using DotnetNiger.Domain.DTOs.Requests;

namespace DotnetNiger.Infrastructure.Services;

public interface IContactService
{
    Task<bool> SendAsync(ContactRequest request);
}
