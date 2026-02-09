using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IAuthService
{
	// Flux d'inscription.
	Task<AuthDto> RegisterAsync(RegisterRequest request);
	// Flux de connexion.
	Task<AuthDto> LoginAsync(LoginRequest request);
}
