namespace DotnetNiger.Community.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PostCount { get; set; } = 0;

    // Relations
    public ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
}
