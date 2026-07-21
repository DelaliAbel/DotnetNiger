namespace DotnetNiger.Api.Entities;

public class Event
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public Guid OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RegisteredCount { get; set; }
    public bool IsPublished { get; set; }
    public bool IsArchived { get; set; }
    public bool IsDeleted { get; set; }
    public string MeetupLink { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? Organizer { get; set; }
    public ICollection<EventMedia> Medias { get; set; } = [];
    public ICollection<EventRegistration> Registrations { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<EventTag> EventTags { get; set; } = [];
    public ICollection<Speaker> Speakers { get; set; } = [];
}

public enum EventStatus
{
    Draft,
    PendingReview,
    Published,
    Cancelled,
    Archived
}
