namespace DotnetNiger.Domain.DTOs.Responses;

public record RecoveryCodesResponse(
    IList<string> RecoveryCodes,
    int RemainingCount);
