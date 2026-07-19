using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête de création d'un projet.</summary>
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

/// <summary>Requête de mise à jour d'un projet.</summary>
public class UpdateProjectRequest
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
