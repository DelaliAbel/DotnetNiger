namespace DotnetNiger.Community.Application.Constants;

/// <summary>
/// Centralized validation constants to avoid magic numbers/strings
/// Used across services, controllers, and DTOs
/// </summary>
public static class ValidationConstants
{
    // ───── Pagination Constants ─────
    /// <summary>Default page size for list endpoints</summary>
    public const int DefaultPageSize = 10;

    /// <summary>Default page for list endpoints</summary>
    public const int DefaultPage = 1;

    /// <summary>Maximum allowed page size to prevent memory abuse</summary>
    public const int MaxPageSize = 100;

    /// <summary>Minimum allowed page size</summary>
    public const int MinPageSize = 1;

    // ───── Title/Name Length Constants ─────
    /// <summary>Maximum length for post titles</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Minimum length for post titles</summary>
    public const int MinTitleLength = 3;

    /// <summary>Maximum length for descriptions/content</summary>
    public const int MaxDescriptionLength = 5000;

    /// <summary>Maximum length for category names</summary>
    public const int MaxCategoryNameLength = 100;

    /// <summary>Maximum length for tag names</summary>
    public const int MaxTagNameLength = 50;

    // ───── Content Length Constants ─────
    /// <summary>Maximum length for comment content</summary>
    public const int MaxCommentLength = 1000;

    /// <summary>Minimum length for meaningful content</summary>
    public const int MinContentLength = 10;

    // ───── URL Length Constants ─────
    /// <summary>Maximum length for URLs (standard web limit)</summary>
    public const int MaxUrlLength = 2048;

    // ───── Search Constants ─────
    /// <summary>Minimum query length for search (prevent DB overload)</summary>
    public const int MinSearchQueryLength = 2;

    /// <summary>Maximum query length for search</summary>
    public const int MaxSearchQueryLength = 255;

    /// <summary>Default page size for search results</summary>
    public const int SearchDefaultPageSize = 20;

    // ───── Status/Approval Constants ─────
    /// <summary>Resource approval status: Pending</summary>
    public const string StatusPending = "Pending";

    /// <summary>Resource approval status: Approved</summary>
    public const string StatusApproved = "Approved";

    /// <summary>Resource approval status: Rejected</summary>
    public const string StatusRejected = "Rejected";

    // ───── Cache Constants ─────
    /// <summary>Dashboard cache duration in minutes</summary>
    public const int DashboardCacheDurationMinutes = 5;

    /// <summary>Category list cache duration in minutes</summary>
    public const int CategoryCacheDurationMinutes = 15;

    /// <summary>Search results cache duration in minutes</summary>
    public const int SearchCacheDurationMinutes = 2;

    // ───── HTTP Constants ─────
    /// <summary>Maximum file upload size in bytes (10 MB)</summary>
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;

    /// <summary>Request timeout in seconds</summary>
    public const int RequestTimeoutSeconds = 30;

    // ───── Validation Messages ─────
    public static class ValidationMessages
    {
        public const string RequiredField = "This field is required.";
        public const string InvalidFormat = "The format is invalid.";
        public static readonly string TitleTooLong = $"Title cannot exceed {MaxTitleLength} characters.";
        public static readonly string TitleTooShort = $"Title must be at least {MinTitleLength} characters.";
        public static readonly string ContentTooLong = $"Content cannot exceed {MaxDescriptionLength} characters.";
        public static readonly string SearchQueryTooShort = $"Search query must be at least {MinSearchQueryLength} characters.";
        public const string InvalidPagination = "Page and page size must be positive integers.";
        public static readonly string PageSizeTooLarge = $"Page size cannot exceed {MaxPageSize}.";
    }
}
