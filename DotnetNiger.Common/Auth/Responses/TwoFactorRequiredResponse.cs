namespace DotnetNiger.Common.Auth.Responses;

/// <summary>
/// Réponse indiquant qu'une vérification 2FA est requise.
/// </summary>
public record TwoFactorRequiredResponse(
    bool RequiresTwoFactor,
    string ChallengeToken,
    string? TwoFactorType = "authenticator");
