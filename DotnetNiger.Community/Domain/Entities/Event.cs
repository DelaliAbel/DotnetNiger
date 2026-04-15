namespace DotnetNiger.Community.Domain.Entities;

/// <summary>
/// Represents a community event (e.g. meetup, workshop).
/// </summary>
public class Event
{
    /// <summary>Gets or sets the unique identifier for the event.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the event title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL-friendly slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Gets or sets the full event description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the physical or relative location.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Gets or sets the type of event (Online, Physical, Hybrid).</summary>
    public string EventType { get; set; } = string.Empty; // Online, Physical, Hybrid

    /// <summary>Gets or sets the scheduled start date and time.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Gets or sets the scheduled end date and time.</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Gets or sets the URL for the event's cover image.</summary>
    public string CoverImageUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the ID of the user (from Identity API) who created the event.</summary>
    public Guid CreatedBy { get; set; } // FK Identity API

    /// <summary>Gets or sets the maximum number of attendees allowed.</summary>
    public int Capacity { get; set; }

    /// <summary>Gets or sets the current count of registered attendees.</summary>
    public int RegisteredCount { get; set; } = 0;

    /// <summary>Gets or sets a value indicating whether the event is published to the public.</summary>
    public bool IsPublished { get; set; }

    /// <summary>Gets or sets the UTC date and time the event was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC date and time the event was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Gets or sets the meeting link (e.g. Zoom, Teams) for online/hybrid events.</summary>
    public string MeetupLink { get; set; } = string.Empty; // Zoom, Teams, etc.

    // Relations
    /// <summary>Gets or sets the registrations associated with this event.</summary>
    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();

    /// <summary>Gets or sets the media items (photos/videos) associated with this event.</summary>
    public ICollection<EventMedia> Medias { get; set; } = new List<EventMedia>();
}
