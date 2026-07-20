using DotnetNiger.Domain.DTOs.Requests;

namespace DotnetNiger.Infrastructure.Services.General;

public interface IContactService
{
    Task<bool> SendAsync(ContactRequest request);
}
