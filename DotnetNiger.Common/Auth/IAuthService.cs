using DotnetNiger.Common.Auth.Requests;
using DotnetNiger.Common.Auth.Responses;

namespace DotnetNiger.Common.Auth;

/// <summary>
/// Interface pour les opérations d'authentification.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Connecte un utilisateur avec email et mot de passe.
    /// </summary>
    Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crée un nouveau compte utilisateur.
    /// </summary>
    Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rafraîchit un token d'accès.
    /// </summary>
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirme l'adresse email d'un utilisateur.
    /// </summary>
    Task ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoie un email de réinitialisation de mot de passe.
    /// </summary>
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Réinitialise le mot de passe avec un token.
    /// </summary>
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
