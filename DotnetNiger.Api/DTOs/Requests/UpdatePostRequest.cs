using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public class UpdatePostRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Slug { get; set; }

    public string? Content { get; set; }

    [MaxLength(500)]
    public string? Excerpt { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? PostType { get; set; }

    public List<Guid>? CategoryIds { get; set; }

    public List<string>? TagNames { get; set; }

    public bool? IsPublished { get; set; }
}
