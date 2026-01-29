using System;
using System.Collections.Generic;

namespace DotnetNiger.Community.Domain.Entities;

public class Project
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string GitHubUrl { get; set; } = string.Empty;
	public Guid OwnerId { get; set; } // FK Identity API
	public bool IsFeatured { get; set; } = false;
	public int Stars { get; set; } = 0;
	public int ContributorsCount { get; set; } = 0;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }
	public string Language { get; set; } = string.Empty; // C#, .NET
	public string License { get; set; } = string.Empty;

	// Relations
	public ICollection<ProjectContributor> Contributors { get; set; } = new List<ProjectContributor>();
}
