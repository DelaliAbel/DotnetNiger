namespace DotnetNiger.Common.Auth.Requests;

/// <summary>
/// Requête d'utilisation d'un code de récupération 2FA.
/// </summary>
public class TwoFactorRecoveryCodeRequest
{
    public string RecoveryCode { get; set; } = string.Empty;
    public string ChallengeToken { get; set; } = string.Empty;
}
