namespace DotnetNiger.Common.Auth.Requests;

/// <summary>
/// Requête de connexion par email et mot de passe.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public Guid? TenantId { get; set; }
}
