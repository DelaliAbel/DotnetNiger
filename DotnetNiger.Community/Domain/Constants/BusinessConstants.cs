namespace DotnetNiger.Community.Domain.Constants;

/// <summary>
/// Centralized constants for business logic, validation rules, and configuration values
/// </summary>
public static class BusinessConstants
{
    /// <summary>
    /// Role constants
    /// </summary>
    public static class Roles
    {
        public const string Admin = "admin";
        public const string Member = "member";
        public const string Guest = "guest";
    }

    /// <summary>
    /// Pagination constants
    /// </summary>
    public static class Pagination
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
        public const int MinPageSize = 1;
        public const int DefaultPage = 1;
    }

    /// <summary>
    /// Post type constants
    /// </summary>
    public static class PostTypes
    {
        public const string Article = "Article";
        public const string Tutorial = "Tutorial";
        public const string News = "News";
        public const string Announcement = "Announcement";
    }

    /// <summary>
    /// Event type constants
    /// </summary>
    public static class EventTypes
    {
        public const string Online = "Online";
        public const string Physical = "Physical";
        public const string Hybrid = "Hybrid";
    }

    /// <summary>
    /// Resource level constants
    /// </summary>
    public static class ResourceLevels
    {
        public const string Beginner = "Beginner";
        public const string Intermediate = "Intermediate";
        public const string Advanced = "Advanced";
    }

    /// <summary>
    /// Resource type constants
    /// </summary>
    public static class ResourceTypes
    {
        public const string Documentation = "Documentation";
        public const string Tutorial = "Tutorial";
        public const string Course = "Course";
        public const string Book = "Book";
        public const string Video = "Video";
        public const string Tool = "Tool";
    }

    /// <summary>
    /// Partner type constants
    /// </summary>
    public static class PartnerTypes
    {
        public const string Partner = "Partner";
        public const string Sponsor = "Sponsor";
    }

    /// <summary>
    /// Validation message constants
    /// </summary>
    public static class ValidationMessages
    {
        public const string RequiredField = "{0} is required.";
        public const string InvalidLength = "{0} must be between {1} and {2} characters.";
        public const string InvalidUrl = "{0} must be a valid URL.";
        public const string InvalidEmail = "{0} must be a valid email address.";
        public const string MinLength = "{0} must be at least {1} characters.";
        public const string MaxLength = "{0} must not exceed {1} characters.";
    }

    /// <summary>
    /// Validation length constants
    /// </summary>
    public static class ValidationLengths
    {
        public const int PostTitleMin = 3;
        public const int PostTitleMax = 200;
        public const int PostExcerptMax = 500;

        public const int EventTitleMin = 3;
        public const int EventTitleMax = 200;

        public const int ResourceTitleMin = 3;
        public const int ResourceTitleMax = 200;

        public const int ProjectNameMin = 3;
        public const int ProjectNameMax = 200;

        public const int PartnerNameMin = 3;
        public const int PartnerNameMax = 200;

        public const int CommentContentMin = 1;
        public const int CommentContentMax = 5000;

        public const int SearchQueryMin = 2;
        public const int SearchQueryMax = 100;

        public const int CategoryNameMin = 3;
        public const int CategoryNameMax = 100;

        public const int TagNameMin = 2;
        public const int TagNameMax = 50;
    }

    /// <summary>
    /// Cache key constants
    /// </summary>
    public static class CacheKeys
    {
        public const string PublishedPostsPrefix = "posts:published:";
        public const string EventsPrefix = "events:";
        public const string ResourcesPrefix = "resources:";
        public const string ProjectsPrefix = "projects:";
        public const string CategoriesPrefix = "categories:";
        public const string TagsPrefix = "tags:";
        public const string StatsKey = "community:stats";
        public const string CacheDurationMinutes = "cache:duration_minutes";
    }

    /// <summary>
    /// Date/time constants
    /// </summary>
    public static class DateTime
    {
        public const int CacheDurationMinutes = 15;
        public const int StatsCacheDurationMinutes = 60;
        public const int SearchCacheDurationMinutes = 30;
    }
}
