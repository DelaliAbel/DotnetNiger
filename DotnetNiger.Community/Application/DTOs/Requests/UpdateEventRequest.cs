namespace DotnetNiger.Community.Application.DTOs.Requests;

public class UpdateEventRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? EventType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CoverImageUrl { get; set; }
    public int? Capacity { get; set; }
    public string? MeetupLink { get; set; }
    public bool? IsPublished { get; set; }
}
