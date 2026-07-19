using DotnetNiger.Client.Helpers;
using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Auth;
using DotnetNiger.Client.Services.Contracts;
using DotnetNiger.Client.Services.Helpers;

namespace DotnetNiger.Client.Services.Mock;

public partial class MockAuthService : IAuthService
{
    private static readonly List<UserDto> _users = MockDataStore.Users;

    private static Dictionary<string, string> _refreshTokens = new();

    private readonly CustomAuthStateProvider _authProvider;
    private readonly IUserStateService _userStateService;
    private readonly IPermissionService _permissionService;
    private UserDto? _currentUser;
    private TokenDto? _currentToken;
    private DateTime? _tokenExpiry;

    public MockAuthService(CustomAuthStateProvider authProvider, IUserStateService userStateService, IPermissionService permissionService)
    {
        _authProvider = authProvider;
        _userStateService = userStateService;
        _permissionService = permissionService;
    }

    #region Authentification

    public async Task<ApiSuccessResponse<AuthDto>> LoginAsync(LoginRequest request)
    {
        await Task.Delay(600);

        var user = _users.FirstOrDefault(u => 
            u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

        if (user == null || request.Password != "password123")
        {
            return new ApiSuccessResponse<AuthDto>
            {
                Success = false,
                Message = "Email ou mot de passe incorrect"
            };
        }

        if (!user.IsActive)
        {
            return new ApiSuccessResponse<AuthDto>
            {
                Success = false,
                Message = "Votre compte est désactivé. Veuillez contacter l'administrateur."
            };
        }

        _currentUser = user;
        _currentToken = GenerateTokenDto(user);
        _tokenExpiry = DateTime.Now.AddSeconds(_currentToken.ExpiresIn);

        user.LastLoginAt = DateTime.Now;
        
        // Stocker le refresh token
        _refreshTokens[user.Id.ToString()] = _currentToken.RefreshToken;

        await _authProvider.SaveTokensAsync(_currentToken.AccessToken, _currentToken.RefreshToken);
        await _permissionService.LoadPermissionsAsync();

        return new ApiSuccessResponse<AuthDto>
        {
            Success = true,
            Message = "Connexion réussie",
            Data = new AuthDto
            {
                User = user,
                Token = _currentToken
            }
        };
    }

    public async Task<ApiSuccessResponse<AuthDto>> CompleteExternalLoginAsync(string ticket)
    {
        await Task.Delay(600);
        return new ApiSuccessResponse<AuthDto>
        {
            Success = false,
            Message = "Le login externe n'est pas disponible en mode mock."
        };
    }

    public async Task<ApiSuccessResponse<AuthDto>> RegisterAsync(RegisterRequest request)
    {
        await Task.Delay(800);

        // Vérifier si l'email existe déjà
        if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return new ApiSuccessResponse<AuthDto>
            {
                Success = false,
                Message = "Cet email est déjà utilisé"
            };
        }


        // Créer un nouvel utilisateur
        var newUser = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            IsActive = true,
            CreatedAt = DateTime.Now,
            Roles = new List<string> { RoleConstants.User },
            Skills = new List<string>()
        };

        _users.Add(newUser);

        return new ApiSuccessResponse<AuthDto>
        {
            Success = true,
            Message = "Inscription réussie. Veuillez vérifier votre email.",
            Data = null
        };
    }

    public string? GetRoleFromAccessToken(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return null;

        var segments = accessToken.Split('.');
        if (segments.Length < 2)
            return null;

        try
        {
            var payloadJson = System.Text.Encoding.UTF8.GetString(JwtParser.ParseBase64WithoutPadding(segments[1]));
            using var document = System.Text.Json.JsonDocument.Parse(payloadJson);
            var root = document.RootElement;

            if (root.TryGetProperty("role", out var roleElement) && roleElement.ValueKind == System.Text.Json.JsonValueKind.String)
                return roleElement.GetString();

            if (root.TryGetProperty("roles", out var rolesElement))
            {
                if (rolesElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    return rolesElement.EnumerateArray().Select(x => x.GetString()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                if (rolesElement.ValueKind == System.Text.Json.JsonValueKind.String)
                    return rolesElement.GetString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        await Task.Delay(200);
        
        if (_currentUser != null)
        {
            _refreshTokens.Remove(_currentUser.Id.ToString());
        }
        
        _currentUser = null;
        _currentToken = null;
        _tokenExpiry = null;

        await _authProvider.ClearTokensAsync();
        await _userStateService.ClearUserAsync();
    }

    #endregion
}
