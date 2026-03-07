// Contrat applicatif Identity: IAuthService
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;

namespace DotnetNiger.Identity.Application.Services.Interfaces;

// Contrat pour les operations d'authentification.
public interface IAuthService
{
	// Flux d'inscription.
	Task<AuthDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
	// Flux de connexion.
	Task<AuthDto> LoginAsync(LoginRequest request, CancellationToken ct = default);
	// Renvoi du token de verification d'email.
	Task<string?> RequestEmailVerificationAsync(RequestEmailVerificationRequest request, CancellationToken ct = default);
	// Demande de reinitialisation du mot de passe.
	Task<string?> RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken ct = default);
	// Reinitialisation du mot de passe avec token.
	Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
	// Verification de l'email.
	Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);
}
