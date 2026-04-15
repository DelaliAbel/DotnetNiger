using DotnetNiger.Identity.Domain.Enums;

namespace DotnetNiger.Identity.Domain.Entities;

public class AccountDeletionRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Reason { get; set; } = string.Empty;
    public AccountDeletionRequestStatus Status { get; set; } = AccountDeletionRequestStatus.Pending;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime ScheduledDeletionAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }
    public ApplicationUser? ReviewedByUser { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewReason { get; set; }

    public DateTime? ExecutedAt { get; set; }
}
