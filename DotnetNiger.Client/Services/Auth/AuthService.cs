using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DotnetNiger.Client.Helpers;
using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Api;
using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Auth;

public partial class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authProvider;
    private readonly IUserStateService _userStateService;
    private readonly IPermissionService _permissionService;
    private readonly string _clientId;

    public AuthService(HttpClient http, CustomAuthStateProvider authProvider, IUserStateService userStateService, IPermissionService permissionService, string clientId = "web-ui")
    {
        _http = http;
        _authProvider = authProvider;
        _userStateService = userStateService;
        _permissionService = permissionService;
        _clientId = clientId;
    }

    public async Task<ApiSuccessResponse<AuthDto>> LoginAsync(LoginRequest request)
    {
        try
        {
            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = request.Email,
                ["password"] = request.Password,
                ["scope"] = "openid profile email roles offline_access",
                ["client_id"] = _clientId
            };

            var response = await _http.PostAsync(ApiEndpoints.Auth.Token, new FormUrlEncodedContent(formData));

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var message = TryReadOidcError(errorBody);

                return new ApiSuccessResponse<AuthDto>
                {
                    Success = false,
                    Message = message ?? $"Connexion impossible (HTTP {(int)response.StatusCode})."
                };
            }

            var (authDto, error) = await ParseTokenResponseAsync(response);
            if (authDto is not null)
            {
                if (authDto.Token is not null)
                    await _authProvider.SaveTokensAsync(authDto.Token.AccessToken, authDto.Token.RefreshToken);
                if (authDto.User is not null)
                    await _userStateService.SetUserAsync(authDto.User);
                await _permissionService.LoadPermissionsAsync();
                return new ApiSuccessResponse<AuthDto> { Success = true, Data = authDto };
            }

            return new ApiSuccessResponse<AuthDto> { Success = false, Message = error ?? "Erreur de connexion." };
        }
        catch (HttpRequestException ex)
        {
            return new()
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (TaskCanceledException)
        {
            return new ApiSuccessResponse<AuthDto>
            {
                Success = false,
                Message = "Le serveur a mis trop de temps à répondre."
            };
        }
    }

    public async Task<ApiSuccessResponse<AuthDto>> CompleteExternalLoginAsync(string ticket)
    {
        try
        {
            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "external_login",
                ["ticket"] = ticket,
                ["client_id"] = _clientId,
                ["scope"] = "openid profile email roles offline_access"
            };

            var response = await _http.PostAsync(ApiEndpoints.Auth.Token, new FormUrlEncodedContent(formData));

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var message = TryReadOidcError(errorBody);
                return new ApiSuccessResponse<AuthDto>
                {
                    Success = false,
                    Message = message ?? "Erreur lors de la connexion externe."
                };
            }

            var (authDto, error) = await ParseTokenResponseAsync(response);
            if (authDto is not null)
            {
                if (authDto.Token is not null)
                    await _authProvider.SaveTokensAsync(authDto.Token.AccessToken, authDto.Token.RefreshToken);
                if (authDto.User is not null)
                    await _userStateService.SetUserAsync(authDto.User);
                await _permissionService.LoadPermissionsAsync();
                return new ApiSuccessResponse<AuthDto> { Success = true, Data = authDto };
            }

            return new ApiSuccessResponse<AuthDto> { Success = false, Message = error ?? "Erreur de connexion externe." };
        }
        catch (HttpRequestException ex)
        {
            return new() { Success = false, Message = ex.Message };
        }
        catch (TaskCanceledException)
        {
            return new ApiSuccessResponse<AuthDto> { Success = false, Message = "Le serveur a mis trop de temps à répondre." };
        }
    }

    public async Task<ApiSuccessResponse<AuthDto>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var names = (request.FullName ?? "").Split(' ', 2, StringSplitOptions.TrimEntries);
            var registerPayload = new
            {
                email = request.Email,
                password = request.Password,
                firstName = names.Length > 0 ? names[0] : "",
                lastName = names.Length > 1 ? names[1] : ""
            };

            var response = await _http.PostAsJsonAsync(ApiEndpoints.Auth.Register, registerPayload);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await TryReadErrorMessageAsync(response.Content);

                return new ApiSuccessResponse<AuthDto>
                {
                    Success = false,
                    Message = !string.IsNullOrWhiteSpace(errorMessage)
                        ? errorMessage
                        : $"Inscription impossible (HTTP {(int)response.StatusCode})."
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var userId = root.TryGetProperty("userId", out var uidProp)
                && uidProp.ValueKind == JsonValueKind.String
                ? uidProp.GetString() : null;
            var email = root.TryGetProperty("email", out var emailProp)
                && emailProp.ValueKind == JsonValueKind.String
                ? emailProp.GetString() : null;
            var message = root.TryGetProperty("message", out var msgProp)
                && msgProp.ValueKind == JsonValueKind.String
                ? msgProp.GetString() : "Compte créé. Vérifiez votre email pour le confirmer.";

            return new ApiSuccessResponse<AuthDto>
            {
                Success = true,
                Message = message,
                Data = new AuthDto
                {
                    User = new UserDto
                    {
                        Id = Guid.TryParse(userId, out var uid) ? uid : Guid.Empty,
                        Email = email ?? request.Email,
                        FullName = request.FullName ?? "",
                        Username = request.FullName ?? ""
                    }
                }
            };
        }
        catch (HttpRequestException ex)
        {
            return new ApiSuccessResponse<AuthDto>
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (TaskCanceledException)
        {
            return new ApiSuccessResponse<AuthDto>
            {
                Success = false,
                Message = "Le serveur a mis trop de temps à répondre."
            };
        }
    }

    public async Task LogoutAsync()
    {
        var refreshToken = await _authProvider.GetRefreshTokenAsync();

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _http.PostAsJsonAsync(ApiEndpoints.Auth.Logout,
                new RefreshTokenRequest { RefreshToken = refreshToken, ClientId = _clientId });
        }

        await _authProvider.ClearTokensAsync();
        await _userStateService.ClearUserAsync();
    }
}
