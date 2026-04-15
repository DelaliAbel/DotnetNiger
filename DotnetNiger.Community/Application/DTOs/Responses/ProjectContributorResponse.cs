namespace DotnetNiger.Community.Application.DTOs.Responses;

public class ProjectContributorResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Contributions { get; set; }
    public DateTime JoinedAt { get; set; }
}
