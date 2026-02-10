using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

public interface IAuthService
{
	// Flux d'inscription.
	Task<AuthDto> RegisterAsync(RegisterRequest request);
	// Flux de connexion.
	Task<AuthDto> LoginAsync(LoginRequest request);
	// Demande de reinitialisation du mot de passe.
	Task<string?> RequestPasswordResetAsync(ForgotPasswordRequest request);
	// Reinitialisation du mot de passe avec token.
	Task ResetPasswordAsync(ResetPasswordRequest request);
	// Verification de l'email.
	Task VerifyEmailAsync(VerifyEmailRequest request);
}
