namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse complète d'un événement.</summary>
public record EventResponse(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    string? CoverImageUrl,
    Guid CreatedBy,
    string Status,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string EventType,
    string Category,
    string OrganizerName,
    int Capacity,
    int RegisteredCount,
    string MeetupLink,
    string? RejectionReason,
    DateTime? SubmittedAt,
    DateTime? PublishedAt,
    List<EventMediaResponse> Medias,
    List<string> GalleryImageUrls,
    List<TagResponse> Tags,
    List<SpeakerResponse> Speakers);

public record EventMediaResponse(Guid Id, string Type, string FileUrl, string Url, string Title);
public record SpeakerResponse(Guid UserId, string Name, string Role, string AvatarUrl);
