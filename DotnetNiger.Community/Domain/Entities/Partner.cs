namespace DotnetNiger.Community.Domain.Entities;

public class Partner
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PartnerType { get; set; } = string.Empty; // Partner, Sponsor
    public string Level { get; set; } = string.Empty; // Gold, Silver, Bronze, Community
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
}
