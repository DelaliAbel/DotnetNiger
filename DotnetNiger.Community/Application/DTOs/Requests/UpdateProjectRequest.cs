namespace DotnetNiger.Community.Application.DTOs.Requests;

public class UpdateProjectRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? GitHubUrl { get; set; }
    public string? Language { get; set; }
    public string? License { get; set; }
    public bool? IsFeatured { get; set; }
}
