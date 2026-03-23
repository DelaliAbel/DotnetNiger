namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>
/// Response DTO for community feature settings.
/// Shows the current state of all community features.
/// </summary>
public record CommunityFeatureSettingsDto
{
    /// <summary>Gets whether posts creation is enabled.</summary>
    public bool PostsEnabled { get; init; }

    /// <summary>Gets whether comments are enabled.</summary>
    public bool CommentsEnabled { get; init; }

    /// <summary>Gets whether events creation is enabled.</summary>
    public bool EventsEnabled { get; init; }

    /// <summary>Gets whether projects creation is enabled.</summary>
    public bool ProjectsEnabled { get; init; }

    /// <summary>Gets whether resources creation is enabled.</summary>
    public bool ResourcesEnabled { get; init; }

    /// <summary>Gets whether search functionality is enabled.</summary>
    public bool SearchEnabled { get; init; }

    /// <summary>Gets whether public registrations to community are enabled.</summary>
    public bool PublicAccessEnabled { get; init; }

    /// <summary>Gets the last update timestamp (UTC).</summary>
    public DateTime UpdatedAtUtc { get; init; }
}
