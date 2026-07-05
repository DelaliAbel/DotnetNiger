namespace DotnetNiger.Common.Auth.Requests;

/// <summary>
/// Requête pour confirmer l'adresse email.
/// </summary>
public class ConfirmEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
