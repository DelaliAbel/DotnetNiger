using DotnetNiger.Common.Auth;
using DotnetNiger.Common.Auth.Requests;
using DotnetNiger.Common.Auth.Responses;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Identity.Application.Services;

/// <summary>Implémentation de <see cref="IAuthService"/> (contrat Common) déléguant aux services internes Identity.</summary>
partial class AuthService
{
    /// <summary>Authentifie un utilisateur par email/mot de passe. Les tokens d'accès sont obtenus via le flux OpenIddict standard.</summary>
    async Task<TokenResponse> IAuthService.LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var (user, roles) = await ValidateCredentialsAsync(request.Email, request.Password);
        return new TokenResponse
        {
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Scope = "openid profile email roles offline_access"
        };
    }

    /// <summary>Crée un compte utilisateur via AccountService. L'utilisateur reçoit un email de confirmation avant de pouvoir se connecter.</summary>
    async Task<TokenResponse> IAuthService.RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = await _accountService.RegisterAsync(request.Email, request.Password, request.FirstName, request.LastName);
        return new TokenResponse
        {
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Scope = "openid profile email roles offline_access"
        };
    }

    /// <summary>Le rafraîchissement des tokens est géré par le provider OpenIddict.</summary>
    Task<TokenResponse> IAuthService.RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Le rafraîchissement du token est géré automatiquement.");

    /// <summary>Confirme l'email d'un utilisateur via AccountService.</summary>
    async Task IAuthService.ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new KeyNotFoundException("Utilisateur introuvable");
        await _accountService.ConfirmEmailAsync(user.Email!, request.Code);
    }

    /// <summary>Envoie un email de réinitialisation de mot de passe via AccountService.</summary>
    async Task IAuthService.ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
        => await _accountService.ForgotPasswordAsync(request.Email);

    /// <summary>Réinitialise le mot de passe via AccountService.</summary>
    async Task IAuthService.ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var error = await _accountService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        if (error != null)
            throw new InvalidOperationException(error);
    }
}
