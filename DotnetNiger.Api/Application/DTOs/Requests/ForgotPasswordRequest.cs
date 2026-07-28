namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de demande de réinitialisation du mot de passe.</summary>
public class ForgotPasswordRequest
{
    /// <summary>Adresse e-mail de l'utilisateur.</summary>
    public string Email { get; set; } = string.Empty;
}
