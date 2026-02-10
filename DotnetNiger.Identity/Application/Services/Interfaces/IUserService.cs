using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IUserService
{
	// Profil utilisateur courant.
	Task<UserDto> GetProfileAsync(Guid userId);

	// Mise a jour du profil utilisateur.
	Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);

	// Changement de mot de passe.
	Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);

	// Changement d'email.
	Task ChangeEmailAsync(Guid userId, ChangeEmailRequest request);
}
