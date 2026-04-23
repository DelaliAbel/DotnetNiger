namespace DotnetNiger.Community.Domain.Entities;

using DotnetNiger.Community.Domain.Interfaces;

public class Resource : ISoftDeletable
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty; // Tutorial, Course, Tool, Library, Documentation
    public string Level { get; set; } = string.Empty; // Beginner, Intermediate, Advanced
    public Guid CreatedBy { get; set; } // FK Identity API
    public bool IsApproved { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Relations
    public ICollection<ResourceCategory> ResourceCategories { get; set; } = new List<ResourceCategory>();
}
