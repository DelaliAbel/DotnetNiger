namespace DotnetNiger.UI.Models.Responses;

public class CertificateAdminDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string CertificateUrl { get; set; } = string.Empty;
    public string CertificateType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public string? ReviewedNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
