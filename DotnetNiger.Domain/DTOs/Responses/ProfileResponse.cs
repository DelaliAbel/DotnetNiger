namespace DotnetNiger.Domain.DTOs.Responses;

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsTeamMember { get; set; }
    public string Position { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Skills { get; set; } = [];
    public List<string> Roles { get; set; } = [];
    public List<SocialLinkResponse> SocialLinks { get; set; } = [];
    public CertificateInfo? Certificate { get; set; }
}

public class CertificateInfo
{
    public string Status { get; set; } = string.Empty;
    public string CertificateType { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public string? ReviewedNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
