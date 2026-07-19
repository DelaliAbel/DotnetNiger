namespace DotnetNiger.Domain.DTOs.Responses;

public record TwoFactorStatusResponse(
    bool IsEnabled,
    bool IsMachineRemembered,
    int RecoveryCodesLeft);
