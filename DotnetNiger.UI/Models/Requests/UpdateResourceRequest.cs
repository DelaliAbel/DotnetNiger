namespace DotnetNiger.UI.Models.Requests;

public class UpdateResourceRequest
{
    public string? Title { get; set; }

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public string? DownloadUrl { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? ResourceType { get; set; }

    public string? Level { get; set; }

    public List<Guid>? CategoryIds { get; set; }

    public List<Guid>? TagIds { get; set; }

    public List<string>? TagNames { get; set; }
}
