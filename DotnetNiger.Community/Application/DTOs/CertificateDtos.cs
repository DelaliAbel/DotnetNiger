namespace DotnetNiger.Community.Application.DTOs;

/// <summary>DTO pour l'affichage admin des certificats avec infos utilisateur.</summary>
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
