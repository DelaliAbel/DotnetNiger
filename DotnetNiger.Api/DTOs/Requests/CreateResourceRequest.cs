using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public class CreateResourceRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required, Url]
    public string Url { get; set; } = string.Empty;

    public string? DownloadUrl { get; set; }

    public string? ThumbnailUrl { get; set; }

    [Required]
    public string ResourceType { get; set; } = string.Empty;

    [Required]
    public string Level { get; set; } = string.Empty;

    public List<Guid> CategoryIds { get; set; } = [];
    public List<Guid> TagIds { get; set; } = [];
    public List<string> TagNames { get; set; } = [];
}
