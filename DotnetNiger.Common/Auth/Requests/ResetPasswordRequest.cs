namespace DotnetNiger.Common.Auth.Requests;

/// <summary>
/// Requête pour réinitialiser le mot de passe avec un token.
/// </summary>
public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
}
