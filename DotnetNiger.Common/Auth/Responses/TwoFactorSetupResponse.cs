namespace DotnetNiger.Common.Auth.Responses;

/// <summary>
/// Réponse contenant les informations de configuration 2FA.
/// </summary>
public record TwoFactorSetupResponse(
    string SharedKey,
    string AuthenticatorUri,
    bool IsEnabled);
