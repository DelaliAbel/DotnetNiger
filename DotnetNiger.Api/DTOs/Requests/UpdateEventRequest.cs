using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public class UpdateEventRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Location { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? EventType { get; set; }

    public string? Category { get; set; }

    public string? OrganizerName { get; set; }

    public int? Capacity { get; set; }

    public string? MeetupLink { get; set; }

    public bool? IsPublished { get; set; }

    public bool? IsArchived { get; set; }

    public List<string>? TagNames { get; set; }

    public List<string>? GalleryImageUrls { get; set; }

    public List<SpeakerRequest>? Speakers { get; set; }
}
