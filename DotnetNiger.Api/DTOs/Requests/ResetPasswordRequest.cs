namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de réinitialisation du mot de passe.</summary>
public class ResetPasswordRequest
{
    /// <summary>Adresse e-mail de l'utilisateur.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Token de réinitialisation reçu par e-mail.</summary>
    public string Token { get; set; } = string.Empty;
    /// <summary>Nouveau mot de passe.</summary>
    public string NewPassword { get; set; } = string.Empty;
}
