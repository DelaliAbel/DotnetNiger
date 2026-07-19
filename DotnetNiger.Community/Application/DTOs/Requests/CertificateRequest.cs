namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête de soumission d'un certificat.</summary>
public class CertificateSubmissionRequest
{
    public Guid UserId { get; set; }
    public string CertificateUrl { get; set; } = string.Empty;
    public string CertificateType { get; set; } = string.Empty;
}
