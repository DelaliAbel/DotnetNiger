namespace DotnetNiger.Api.Entities;

public class AccountDeletionRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime ScheduledFor { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool IsProcessed { get; set; }

    public ApplicationUser? User { get; set; }
}
