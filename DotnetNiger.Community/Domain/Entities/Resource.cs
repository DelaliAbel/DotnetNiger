using System;
using System.Collections.Generic;

namespace DotnetNiger.Community.Domain.Entities;

public class Resource
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

	// Relations
	public ICollection<ResourceCategory> ResourceCategories { get; set; } = new List<ResourceCategory>();
}
