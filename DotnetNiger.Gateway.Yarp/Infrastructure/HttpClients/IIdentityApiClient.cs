namespace DotnetNiger.Gateway.Infrastructure.HttpClients;

/// <summary>
/// Interface pour le client API Identity Service
/// </summary>
public interface IIdentityApiClient
{
    /// <summary>
    /// Valide un token JWT
    /// </summary>
    Task<bool> ValidateTokenAsync(string token);

    /// <summary>
    /// Récupère les informations de l'utilisateur actuel
    /// </summary>
    Task<UserInfoResponse?> GetCurrentUserAsync(string token);

    /// <summary>
    /// Récupère les informations d'un utilisateur par ID
    /// </summary>
    Task<UserInfoResponse?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Récupère la liste des rôles
    /// </summary>
    Task<List<RoleResponse>?> GetRolesAsync();

    /// <summary>
    /// Authentifie un utilisateur
    /// </summary>
    Task<AuthResponse?> AuthenticateAsync(LoginRequest request);

    /// <summary>
    /// Enregistre un nouvel utilisateur
    /// </summary>
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Rafraîchit le token
    /// </summary>
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
}

/// <summary>
/// Données de réponse pour les informations utilisateur
/// </summary>
public class UserInfoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Données de réponse pour les rôles
/// </summary>
public class RoleResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Données de réponse d'authentification
/// </summary>
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserInfoResponse? User { get; set; }
    public DateTime ExpiresIn { get; set; }
}

/// <summary>
/// Données de requête de connexion
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Données de requête d'enregistrement
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
