using System;

namespace DotnetNiger.Community.Domain.Entities;

public class PostCategory
{
	public Guid PostId { get; set; }
	public Guid CategoryId { get; set; }

	// FK
	public Post Post { get; set; } = null!;
	public Category Category { get; set; } = null!;
}
