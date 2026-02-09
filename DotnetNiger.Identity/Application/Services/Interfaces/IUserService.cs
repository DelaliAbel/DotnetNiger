using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IUserService
{
	// Profil utilisateur courant.
	Task<UserDto> GetProfileAsync(Guid userId);
}
