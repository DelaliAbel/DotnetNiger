using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public class CreateProjectRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
    public string GithubUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Technologies { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
}

public class UpdateProjectRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public string? GithubUrl { get; set; }

    public string? ImageUrl { get; set; }

    public string? Technologies { get; set; }

    public string? Status { get; set; }

    public bool? IsFeatured { get; set; }

    public bool? IsPublished { get; set; }
}
