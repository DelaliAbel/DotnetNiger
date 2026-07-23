namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de connexion utilisateur.</summary>
public class LoginRequest
{
    /// <summary>Adresse e-mail de l'utilisateur.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Mot de passe de l'utilisateur.</summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>Indique si la session doit être maintenue.</summary>
    public bool RememberMe { get; set; }
}
