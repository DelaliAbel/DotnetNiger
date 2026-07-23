namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de confirmation d'adresse e-mail.</summary>
public class ConfirmEmailRequest
{
    /// <summary>Adresse e-mail à confirmer.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Code de confirmation reçu par e-mail.</summary>
    public string Code { get; set; } = string.Empty;
}
