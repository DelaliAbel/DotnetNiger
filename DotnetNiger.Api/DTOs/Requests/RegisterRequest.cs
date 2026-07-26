using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête d'inscription d'un nouvel utilisateur.</summary>
public class RegisterRequest
{
    /// <summary>Prénom de l'utilisateur.</summary>
    [Required(ErrorMessage = "Le prénom est requis.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Le prénom doit contenir au moins 1 caractère.")]
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Nom de famille de l'utilisateur.</summary>
    [Required(ErrorMessage = "Le nom est requis.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Le nom doit contenir au moins 1 caractère.")]
    public string LastName { get; set; } = string.Empty;
    /// <summary>Adresse e-mail.</summary>
    [Required(ErrorMessage = "L'email est requis.")]
    [EmailAddress(ErrorMessage = "Adresse email invalide.")]
    public string Email { get; set; } = string.Empty;
    /// <summary>Mot de passe.</summary>
    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
    public string Password { get; set; } = string.Empty;
    /// <summary>Numéro de téléphone.</summary>
    public string? PhoneNumber { get; set; }
}
