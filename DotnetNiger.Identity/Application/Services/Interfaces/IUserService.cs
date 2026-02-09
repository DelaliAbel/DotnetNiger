using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IUserService
{
	Task<UserDto> GetProfileAsync(Guid userId);
}
