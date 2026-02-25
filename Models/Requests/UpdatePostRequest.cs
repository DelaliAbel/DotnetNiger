using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

public class UpdatePostRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [StringLength(500)]
    public string Excerpt { get; set; } = string.Empty;

    public string CoverImageUrl { get; set; } = string.Empty;

    [Required]
    public string PostType { get; set; } = string.Empty;

    public string SeoDescription { get; set; } = string.Empty;

    public List<Guid> CategoryIds { get; set; } = new();
    public List<string> TagNames { get; set; } = new();
    public bool IsPublished { get; set; }
}
