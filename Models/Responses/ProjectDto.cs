namespace DotnetNiger.UI.Models.Responses;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GitHubUrl { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerAvatar { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public int Stars { get; set; }
    public int ContributorsCount { get; set; }
    public string Language { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
}
