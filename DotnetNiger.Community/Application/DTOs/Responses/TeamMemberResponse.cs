namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>
/// Data Transfer Object for Member entity.
/// Used in GET endpoints to return member information.
/// </summary>
public class TeamMemberResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string BioOverride { get; set; } = string.Empty;
    public string RoleDescription { get; set; } = string.Empty;
    public List<SocialLinkResponse> SocialLinks { get; set; } = new();
    public List<string> Skills { get; set; } = new();
}
