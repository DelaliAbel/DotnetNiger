namespace DotnetNiger.Community.Domain.Enums;

/// <summary>
/// Post type classifications - must match DomainConstants.PostTypes
/// </summary>
public enum PostType
{
    /// <summary>Blog post - default content type</summary>
    Blog = 0,

    /// <summary>Community news and announcements</summary>
    News = 1,

    /// <summary>Educational tutorial content</summary>
    Tutorial = 2,

    /// <summary>Important announcements</summary>
    Announcement = 3
}
