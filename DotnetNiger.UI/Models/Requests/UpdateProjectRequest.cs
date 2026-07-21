namespace DotnetNiger.UI.Models.Requests;

public class UpdateProjectRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public string? GithubUrl { get; set; }

    public string? ImageUrl { get; set; }

    public string? Technologies { get; set; }

    public string? Status { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; }
}
