namespace DotnetNiger.Domain.Entities;

public class Resource
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? DownloadUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public ResourceStatus Status { get; set; } = ResourceStatus.Draft;
    public Guid CreatedBy { get; set; }
    public Guid AuthorId { get; set; }
    public int ViewCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? Author { get; set; }
    public ICollection<ResourceCategory> ResourceCategories { get; set; } = [];
    public ICollection<ResourceTag> ResourceTags { get; set; } = [];
}

public enum ResourceStatus
{
    Draft,
    PendingReview,
    Published,
    Archived
}
