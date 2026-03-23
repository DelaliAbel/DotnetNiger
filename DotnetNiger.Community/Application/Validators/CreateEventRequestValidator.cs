// Validation Community: CreateEventRequestValidator
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Exceptions;
using DotnetNiger.Community.Domain.Constants;

namespace DotnetNiger.Community.Application.Validators;

/// <summary>
/// Validation specifique pour creation d'event.
/// </summary>
public static class CreateEventRequestValidator
{
    private const int MinTitleLength = 3;
    private const int MaxTitleLength = 200;

    public static void ValidateAndThrow(CreateEventRequest request)
    {
        if (request == null)
            throw new NotFoundException("Create event request is required.");

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < MinTitleLength)
            throw new NotFoundException($"Title must be at least {MinTitleLength} characters.");

        if (request.Title.Length > MaxTitleLength)
            throw new NotFoundException($"Title must not exceed {MaxTitleLength} characters.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new NotFoundException("Description is required.");

        if (string.IsNullOrWhiteSpace(request.EventType))
            throw new NotFoundException("Event type is required.");

        if (!DomainConstants.EventTypes.IsValid(request.EventType))
            throw new NotFoundException($"Event type must be one of: {string.Join(", ", DomainConstants.EventTypes.All)}.");

        if (request.StartDate == default)
            throw new NotFoundException("Start date is required.");

        if (request.EndDate == default)
            throw new NotFoundException("End date is required.");

        if (request.EndDate <= request.StartDate)
            throw new NotFoundException("End date must be after start date.");

        if (request.Capacity <= 0)
            throw new NotFoundException("Capacity must be greater than 0.");

        if (!string.IsNullOrWhiteSpace(request.CoverImageUrl) && !IsValidUrl(request.CoverImageUrl))
            throw new NotFoundException("Cover image URL is invalid.");

        if (request.EventType == "Online" && string.IsNullOrWhiteSpace(request.MeetupLink))
            throw new NotFoundException("Meetup link is required for online events.");

        if (!string.IsNullOrWhiteSpace(request.MeetupLink) && !IsValidUrl(request.MeetupLink))
            throw new NotFoundException("Meetup link URL is invalid.");
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
