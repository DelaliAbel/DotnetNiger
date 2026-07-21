// Services/IAuthService.cs
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;

namespace DotnetNiger.UI.Services.Contracts;

public interface IAuthService
{
    string? GetRoleFromAccessToken(string? accessToken);
    
    // Authentification
    Task<ApiSuccessResponse<AuthDto>> LoginAsync(LoginRequest request);
    Task<ApiSuccessResponse<AuthDto>> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    
    // Gestion de compte
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiSuccessResponse<object>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> RequestEmailVerificationAsync(RequestEmailVerificationRequest request);
    Task<bool> VerifyEmailAsync(VerifyEmailRequest request);
    
    // Login externe (Google/GitHub) - échange du code authorization_code
    Task<ApiSuccessResponse<AuthDto>> CompleteCodeExchangeAsync(string code, string redirectUri);

    // Refresh token
    Task<AuthDto?> RefreshTokenAsync();
    
    // État utilisateur
    Task<UserDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> IsAdminAsync();
    Task<string?> GetAccessTokenAsync();
}