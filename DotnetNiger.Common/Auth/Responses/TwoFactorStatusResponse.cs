namespace DotnetNiger.Common.Auth.Responses;

/// <summary>
/// Réponse indiquant le statut de l'authentification à deux facteurs.
/// </summary>
public record TwoFactorStatusResponse(
    bool IsEnabled,
    bool IsMachineRemembered,
    int RecoveryCodesLeft);
