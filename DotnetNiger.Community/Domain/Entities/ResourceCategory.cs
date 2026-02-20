namespace DotnetNiger.Community.Domain.Entities;

public class ResourceCategory
{
	public Guid ResourceId { get; set; }
	public Guid CategoryId { get; set; }

	// FK
	public Resource Resource { get; set; } = null!;
	public Category Category { get; set; } = null!;
}
