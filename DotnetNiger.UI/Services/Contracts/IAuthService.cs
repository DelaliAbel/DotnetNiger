// Services/IAuthService.cs
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface IAuthService
{
    string? GetRoleFromAccessToken(string? accessToken);
    
    // Authentification
    Task<ApiSuccessResponse<AuthDto>> LoginAsync(LoginRequest request);
    Task<ApiSuccessResponse<AuthDto>> RegisterAsync(RegisterRequest request);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    
    // Gestion de compte
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<ApiSuccessResponse<object>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> RequestEmailVerificationAsync(RequestEmailVerificationRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
    
    // Login externe (Google/GitHub)
    Task<ApiSuccessResponse<AuthDto>> CompleteExternalLoginAsync(string ticket);

    // Refresh token
    Task<AuthDto?> RefreshTokenAsync(CancellationToken cancellationToken = default);
    
    // État utilisateur
    Task<UserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default);
    Task<bool> IsAdminAsync(CancellationToken cancellationToken = default);
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}