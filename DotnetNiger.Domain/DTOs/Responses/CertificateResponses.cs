namespace DotnetNiger.Domain.DTOs.Responses;

public class CertificateResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public string EstimatedWaitTime { get; set; } = "24-48 heures";
    public string SupportEmail { get; set; } = "support@dotnetniger.org";
}
