namespace DotnetNiger.Identity.Infrastructure.External;

// Options de configuration du workflow de suppression de compte.
public class AccountDeletionOptions
{
    public int ApprovalWindowDays { get; set; } = 30;
    public int DefaultExecutionBatchSize { get; set; } = 100;
    public int MaxExecutionBatchSize { get; set; } = 500;
}
