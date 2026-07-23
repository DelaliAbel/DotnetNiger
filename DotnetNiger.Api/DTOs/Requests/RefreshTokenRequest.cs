namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de rafraîchissement du token d'accès.</summary>
public class RefreshTokenRequest
{
    /// <summary>Token de rafraîchissement.</summary>
    public string RefreshToken { get; set; } = string.Empty;
}
