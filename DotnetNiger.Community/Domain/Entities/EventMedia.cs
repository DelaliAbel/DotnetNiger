using System;

namespace DotnetNiger.Community.Domain.Entities;

public class EventMedia
{
	public Guid Id { get; set; }
	public Guid EventId { get; set; }
	public string Type { get; set; } = string.Empty; // Video, Slides, Photos, Recording
	public string Url { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	// FK
	public Event Event { get; set; } = null!;
}
