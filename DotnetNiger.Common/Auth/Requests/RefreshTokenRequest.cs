namespace DotnetNiger.Common.Auth.Requests;

/// <summary>
/// Requête pour rafraîchir un token d'accès.
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
