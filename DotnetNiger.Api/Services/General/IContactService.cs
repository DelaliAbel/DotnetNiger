using DotnetNiger.Api.DTOs.Requests;

namespace DotnetNiger.Api.Services.General;

public interface IContactService
{
    Task<bool> SendAsync(ContactRequest request);
}
