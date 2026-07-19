namespace DotnetNiger.Domain.DTOs.Requests;

public class TwoFactorRecoveryCodeRequest
{
    public string RecoveryCode { get; set; } = string.Empty;
    public string ChallengeToken { get; set; } = string.Empty;
}
