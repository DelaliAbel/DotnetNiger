namespace DotnetNiger.Common.Auth.Requests;

/// <summary>
/// Requête pour demander la réinitialisation du mot de passe.
/// </summary>
public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
