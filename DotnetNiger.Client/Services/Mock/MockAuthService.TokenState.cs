using DotnetNiger.Client.Helpers;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Mock;

public partial class MockAuthService
{
    #region Refresh Token

    public async Task<AuthDto?> RefreshTokenAsync()
    {
        await Task.Delay(500);

        var user = _currentUser;
        if (user is null || _currentToken is null || _tokenExpiry <= DateTime.Now)
            return null;

        var newToken = GenerateTokenDto(user);
        _currentToken = newToken;
        _tokenExpiry = DateTime.Now.AddSeconds(newToken.ExpiresIn);
        _refreshTokens[user.Id.ToString()] = newToken.RefreshToken;

        return new AuthDto { User = user, Token = newToken };
    }

    #endregion

    #region État utilisateur

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        await Task.Delay(100);
        return _currentUser;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        await Task.Delay(50);
        return _currentUser != null && _tokenExpiry > DateTime.Now;
    }

    public async Task<bool> IsAdminAsync()
    {
        await Task.Delay(50);
        return _currentUser?.Roles.Any(r => RoleConstants.IsAdminRole(r)) ?? false;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        await Task.Delay(50);
        
        if (_currentToken == null || _tokenExpiry <= DateTime.Now)
        {
            return null;
        }
        
        return _currentToken.AccessToken;
    }

    #endregion

    #region Méthodes privées

    private TokenDto GenerateTokenDto(UserDto user)
    {
        // Simuler un JWT (token mocké)
        var mockToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            $"{{\"sub\":\"{user.Id}\",\"email\":\"{user.Email}\",\"role\":\"{user.Roles.FirstOrDefault()}\",\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}}"));
        
        return new TokenDto
        {
            AccessToken = $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{mockToken}.mock-signature",
            RefreshToken = GenerateRefreshToken(),
            ExpiresIn = 3600, // 1 heure en secondes
            TokenType = "Bearer"
        };
    }

    private string GenerateRefreshToken()
    {
        return $"refresh-{Guid.NewGuid()}-{DateTime.Now.Ticks}";
    }

    #endregion
}
