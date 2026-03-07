namespace DotnetNiger.Community.Domain.Entities;

public class ProjectContributor
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; } // FK Identity API
    public string Role { get; set; } = string.Empty; // Owner, Maintainer, Contributor
    public int Contributions { get; set; } = 0;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // FK
    public Project Project { get; set; } = null!;
}
