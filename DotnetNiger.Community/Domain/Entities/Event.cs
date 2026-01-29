using System;
using System.Collections.Generic;

namespace DotnetNiger.Community.Domain.Entities;

public class Event
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;
	public string EventType { get; set; } = string.Empty; // Online, Physical, Hybrid
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public string CoverImageUrl { get; set; } = string.Empty;
	public Guid CreatedBy { get; set; } // FK Identity API
	public int Capacity { get; set; }
	public int RegisteredCount { get; set; } = 0;
	public bool IsPublished { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }
	public string MeetupLink { get; set; } = string.Empty; // Zoom, Teams, etc.

	// Relations
	public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
	public ICollection<EventMedia> Medias { get; set; } = new List<EventMedia>();
}
