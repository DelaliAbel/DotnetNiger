using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

public record UpdateAccountDeletionSettingsRequest
{
    [Range(1, 365)]
    public int? ApprovalWindowDays { get; init; }

    [Range(1, 1000)]
    public int? DefaultExecutionBatchSize { get; init; }

    [Range(1, 5000)]
    public int? MaxExecutionBatchSize { get; init; }
}
