namespace DotnetNiger.Community.Domain.Entities;

public class Certificate
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CertificateUrl { get; set; } = string.Empty;
    public string CertificateType { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime SubmissionDate { get; set; }
    public string? ReviewedNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public Member Member { get; set; } = null!;
}
