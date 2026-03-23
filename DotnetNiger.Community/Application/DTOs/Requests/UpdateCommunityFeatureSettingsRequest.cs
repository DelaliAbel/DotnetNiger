namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>
/// Request DTO for updating community feature settings.
/// Allows admins to enable/disable specific features dynamically.
/// </summary>
public record UpdateCommunityFeatureSettingsRequest
{
    /// <summary>Gets whether posts creation is enabled.</summary>
    public bool? PostsEnabled { get; init; }

    /// <summary>Gets whether comments are enabled.</summary>
    public bool? CommentsEnabled { get; init; }

    /// <summary>Gets whether events creation is enabled.</summary>
    public bool? EventsEnabled { get; init; }

    /// <summary>Gets whether projects creation is enabled.</summary>
    public bool? ProjectsEnabled { get; init; }

    /// <summary>Gets whether resources creation is enabled.</summary>
    public bool? ResourcesEnabled { get; init; }

    /// <summary>Gets whether search functionality is enabled.</summary>
    public bool? SearchEnabled { get; init; }

    /// <summary>Gets whether public registrations to community are enabled.</summary>
    public bool? PublicAccessEnabled { get; init; }
}
