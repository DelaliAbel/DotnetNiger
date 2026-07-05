namespace DotnetNiger.Common.Auth.Responses;

/// <summary>
/// Réponse contenant les codes de récupération 2FA.
/// </summary>
public record RecoveryCodesResponse(
    IList<string> RecoveryCodes,
    int RemainingCount);
