namespace DotnetNiger.Domain.DTOs.Requests;

public class CertificateSubmissionRequest
{
    public Guid UserId { get; set; }
    public string CertificateUrl { get; set; } = string.Empty;
    public string CertificateType { get; set; } = string.Empty;
}
