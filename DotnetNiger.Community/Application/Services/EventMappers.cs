using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.Extensions;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Mappers statiques pour les événements et leurs dépendances.</summary>
internal static class EventMappers
{
    /// <summary>Transforme un Event en EventResponse.</summary>
    public static EventResponse ToResponse(Event e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Slug = e.Slug,
        Description = e.Description,
        Location = e.Location,
        EventType = e.EventType,
        Category = e.Category,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        CoverImageUrl = e.CoverImageUrl,
        CreatedBy = e.CreatedBy,
        SubmittedBy = e.CreatedBy,
        OrganizerName = e.OrganizerName,
        Capacity = e.Capacity,
        RegisteredCount = e.RegisteredCount,
        IsPublished = e.IsPublished,
        IsArchived = e.IsArchived,
        MeetupLink = e.MeetupLink,
        RejectionReason = e.RejectionReason,
        SubmittedAt = e.SubmittedAt,
        PublishedAt = e.PublishedAt,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        Medias = MapMedia(e.Medias),
        GalleryImageUrls = e.Medias.Where(m => m.Type == "Image").Select(m => m.Url).ToList(),
        Speakers = MapSpeakers(e.Speakers),
        Tags = MapTags(e.EventTags)
    };

    private static List<EventMediaResponse> MapMedia(ICollection<EventMedia> media) =>
        media.Select(m => new EventMediaResponse
        {
            Id = m.Id,
            Type = m.Type,
            Url = m.Url,
            Title = m.Title
        }).ToList();

    private static List<SpeakerResponse> MapSpeakers(ICollection<Speaker> speakers) =>
        speakers.Select(s => new SpeakerResponse
        {
            Id = s.Id,
            EventId = s.EventId,
            UserId = s.UserId,
            Name = s.Name,
            Role = s.Role,
            AvatarUrl = s.AvatarUrl
        }).ToList();

    private static List<TagResponse> MapTags(ICollection<EventTag> tags) =>
        tags.Select(et => new TagResponse
        {
            Id = et.Tag.Id,
            Name = et.Tag.Name,
            Slug = et.Tag.Slug,
            UsageCount = et.Tag.UsageCount
        }).ToList();

    /// <summary>Transforme une EventRegistration en EventRegistrationResponse.</summary>
    public static EventRegistrationResponse ToRegistrationResponse(EventRegistration r, string eventTitle) => new()
    {
        Id = r.Id,
        EventId = r.EventId,
        EventTitle = eventTitle,
        UserId = r.UserId,
        UserName = r.UserName,
        AvatarUrl = r.AvatarUrl,
        RegisteredAt = r.RegisteredAt,
        IsAttended = r.IsAttended,
        RegistrationStatus = r.RegistrationStatus
    };
}
