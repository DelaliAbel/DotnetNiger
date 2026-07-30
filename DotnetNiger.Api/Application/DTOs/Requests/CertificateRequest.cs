namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de soumission d'un certificat.</summary>
public class CertificateSubmissionRequest
{
    /// <summary>URL du certificat soumis.</summary>
    public string CertificateUrl { get; set; } = string.Empty;
    /// <summary>Type de certificat (AWS, Azure, etc.).</summary>
    public string CertificateType { get; set; } = string.Empty;
}
