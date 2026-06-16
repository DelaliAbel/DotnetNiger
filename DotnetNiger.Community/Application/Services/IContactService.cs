using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

public interface IContactService
{
    Task<bool> SendAsync(ContactRequest request);
}
