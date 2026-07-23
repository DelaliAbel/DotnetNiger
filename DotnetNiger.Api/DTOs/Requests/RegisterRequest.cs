namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête d'inscription d'un nouvel utilisateur.</summary>
public class RegisterRequest
{
    /// <summary>Prénom de l'utilisateur.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Nom de famille de l'utilisateur.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Adresse e-mail.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Mot de passe.</summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>Numéro de téléphone.</summary>
    public string? PhoneNumber { get; set; }
}
