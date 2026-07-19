namespace DotnetNiger.Domain.DTOs.Responses;

public record TwoFactorSetupResponse(
    string SharedKey,
    string AuthenticatorUri,
    bool IsEnabled);
