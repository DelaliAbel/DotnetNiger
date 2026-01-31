namespace DotnetNiger.Community.Domain.Entities;

public class PostTag
{
    public Guid PostId { get; set; }
    public Guid TagId { get; set; }

    // FK
    public Post Post { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
