namespace DotnetNiger.Domain.DTOs.Responses;

public class TeamMemberResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public List<SocialLinkResponse> SocialLinks { get; set; } = [];
}
