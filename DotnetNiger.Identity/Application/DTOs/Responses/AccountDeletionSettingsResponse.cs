namespace DotnetNiger.Identity.Application.DTOs.Responses;

public record AccountDeletionSettingsResponse
{
    public int ApprovalWindowDays { get; init; }
    public int DefaultExecutionBatchSize { get; init; }
    public int MaxExecutionBatchSize { get; init; }
}
