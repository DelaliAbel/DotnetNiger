namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>DTO pour la déconnexion avec révocation du refresh token.</summary>
public class LogoutRequest
{
    /// <summary>Refresh token à révoquer (optionnel, pour la déconnexion de l'appareil).</summary>
    public string? RefreshToken { get; set; }
}
