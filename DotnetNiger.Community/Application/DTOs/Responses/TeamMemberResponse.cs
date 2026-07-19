namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>Données publiques d'un membre de l'équipe DotnetNiger.</summary>
public class TeamMemberResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public List<SocialLinkResponse> SocialLinks { get; set; } = [];
}
