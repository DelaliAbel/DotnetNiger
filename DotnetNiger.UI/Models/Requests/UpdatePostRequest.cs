namespace DotnetNiger.UI.Models.Requests;

public class UpdatePostRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Excerpt { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? PostType { get; set; }
    public string? SeoDescription { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public List<string>? TagNames { get; set; }
    public bool? IsPublished { get; set; }
}
