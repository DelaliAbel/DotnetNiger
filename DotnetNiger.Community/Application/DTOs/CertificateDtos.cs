namespace DotnetNiger.Community.Application.DTOs;

public class CertificateSubmissionRequest
{
    public Guid UserId { get; set; }
    public string CertificateUrl { get; set; } = string.Empty;
    public string CertificateType { get; set; } = string.Empty;
}

public class CertificateResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public string EstimatedWaitTime { get; set; } = "24-48 heures";
    public string SupportEmail { get; set; } = "support@dotnetniger.org";
}
