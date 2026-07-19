using System.Net.Http.Json;
using System.Security.Claims;
using DotnetNiger.Client.Helpers;
using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Api;
using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Auth;

public partial class AuthService
{
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    /// <summary>
    /// Renouvelle l'access token depuis le refresh token stocké.
    /// Efface la session si le refresh token est invalide ou expiré.
    /// </summary>
    public async Task<AuthDto?> RefreshTokenAsync()
    {
        if (!await _refreshLock.WaitAsync(TimeSpan.FromSeconds(5)))
            return null;

        try
        {
            var refreshToken = await _authProvider.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["scope"] = "openid profile email roles offline_access",
                ["client_id"] = _clientId
            };

            var response = await _http.PostAsync(ApiEndpoints.Auth.Token, new FormUrlEncodedContent(formData));

            if (!response.IsSuccessStatusCode)
            {
                await _authProvider.ClearTokensAsync();
                return null;
            }

            var (authDto, _) = await ParseTokenResponseAsync(response);
            if (authDto?.Token is not null)
            {
                await _authProvider.SaveTokensAsync(authDto.Token.AccessToken, authDto.Token.RefreshToken);
                await _permissionService.LoadPermissionsAsync();
            }

            return authDto;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

	public async Task<UserDto?> GetCurrentUserAsync()
	{
		var token = await _authProvider.GetAccessTokenAsync();
		if (string.IsNullOrWhiteSpace(token))
			return null;

		var claims = ParseClaimsFromJwt(token).ToList();
		var userIdClaim = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier || claim.Type == "sub");

		if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
			return null;

		var email = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email || claim.Type == "email")?.Value ?? string.Empty;
		var fullName = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name || claim.Type == "name" || claim.Type == "full_name")?.Value ?? string.Empty;
		var avatarUrl = claims.FirstOrDefault(claim => claim.Type == "avatar_url" || claim.Type == "avatarUrl" || claim.Type == "picture")?.Value ?? string.Empty;
		var roles = claims
			.Where(claim => claim.Type == ClaimTypes.Role)
			.Select(claim => claim.Value)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

		return new UserDto
		{
			Id = userId,
			Email = email,
			FullName = fullName,
			AvatarUrl = avatarUrl,
			Username = string.IsNullOrWhiteSpace(fullName) ? email : fullName,
			IsActive = true,
			Roles = roles
		};
	}

    public async Task<bool> IsAuthenticatedAsync()
        => !string.IsNullOrWhiteSpace(await _authProvider.GetAccessTokenAsync());

    public async Task<bool> IsAdminAsync()
    {
        var token = await _authProvider.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var roles = JwtParser.ParseClaimsFromJwt(token)
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        return roles.Any(r => RoleConstants.IsAdminRole(r));
    }

    public Task<string?> GetAccessTokenAsync()
        => _authProvider.GetAccessTokenAsync();

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var response = await _http.PostAsJsonAsync(ApiEndpoints.Auth.ForgotPassword, request);
        return response.IsSuccessStatusCode;
    }

    public async Task<ApiSuccessResponse<object>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var resetPayload = new { email = request.Email, token = request.Token, password = request.NewPassword };
        var response = await _http.PostAsJsonAsync(ApiEndpoints.Auth.ResetPassword, resetPayload);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await TryReadErrorMessageAsync(response.Content);

            return new ApiSuccessResponse<object>
            {
                Success = false,
                Message = !string.IsNullOrWhiteSpace(errorMessage)
                    ? errorMessage
                    : "Erreur lors de la réinitialisation."
            };
        }

        if (response.Content.Headers.ContentLength is null or 0)
        {
            return new ApiSuccessResponse<object>
            {
                Success = true,
                Message = "Mot de passe réinitialisé avec succès."
            };
        }

        var wrapped = await response.Content.ReadFromJsonAsync<ApiSuccessResponse<object>>();
        return new ApiSuccessResponse<object>
        {
            Success = true,
            Message = wrapped?.Message ?? "Mot de passe réinitialisé avec succès."
        };
    }

    public async Task<bool> RequestEmailVerificationAsync(RequestEmailVerificationRequest request)
    {
        var response = await _http.PostAsJsonAsync(ApiEndpoints.Auth.RequestEmailVerification, request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var response = await _http.PostAsJsonAsync(ApiEndpoints.Auth.VerifyEmail, request);
        return response.IsSuccessStatusCode;
    }
}
