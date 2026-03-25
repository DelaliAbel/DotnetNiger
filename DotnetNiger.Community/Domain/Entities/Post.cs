namespace DotnetNiger.Community.Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Markdown ou HTML
    public string Excerpt { get; set; } = string.Empty; // description
    public string CoverImageUrl { get; set; } = string.Empty;
    public Guid AuthorId { get; set; } // Référence Identity API
    public string PostType { get; set; } = string.Empty; // Blog, News, Interview, Update
    public DateTime PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublished { get; set; }
    public int ViewCount { get; set; } = 0;
    public string SeoDescription { get; set; } = string.Empty;

    // Relations
    public ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
