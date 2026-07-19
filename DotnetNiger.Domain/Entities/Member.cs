namespace DotnetNiger.Domain.Entities;

public class Member
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsTeamMember { get; set; }
    public string Position { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
    public ICollection<SocialLink> SocialLinks { get; set; } = [];
    public ICollection<Certificate> Certificates { get; set; } = [];
    public ICollection<MemberSkill> Skills { get; set; } = [];
}
