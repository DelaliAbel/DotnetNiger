namespace DotnetNiger.Community.Domain.Constants;

/// <summary>
/// Master domain constants consolidating all enum values
/// This is the single source of truth for domain constants
/// </summary>
public static class DomainConstants
{
    /// <summary>
    /// Post type classifications
    /// </summary>
    public static class PostTypes
    {
        public const string Blog = "Blog";
        public const string News = "News";
        public const string Tutorial = "Tutorial";
        public const string Announcement = "Announcement";

        public static readonly string[] All = { Blog, News, Tutorial, Announcement };

        /// <summary>Validates if the given type is a valid PostType</summary>
        public static bool IsValid(string type) => All.Contains(type);
    }

    /// <summary>
    /// Event delivery types
    /// </summary>
    public static class EventTypes
    {
        public const string Online = "Online";
        public const string Physical = "Physical";
        public const string Hybrid = "Hybrid";

        public static readonly string[] All = { Online, Physical, Hybrid };

        /// <summary>Validates if the given type is a valid EventType</summary>
        public static bool IsValid(string type) => All.Contains(type);
    }

    /// <summary>
    /// Resource difficulty levels
    /// </summary>
    public static class ResourceLevels
    {
        public const string Beginner = "Beginner";
        public const string Intermediate = "Intermediate";
        public const string Advanced = "Advanced";

        public static readonly string[] All = { Beginner, Intermediate, Advanced };

        /// <summary>Validates if the given level is a valid ResourceLevel</summary>
        public static bool IsValid(string level) => All.Contains(level);
    }

    /// <summary>
    /// Resource content types
    /// </summary>
    public static class ResourceTypes
    {
        public const string Documentation = "Documentation";
        public const string Tutorial = "Tutorial";
        public const string Course = "Course";
        public const string Book = "Book";
        public const string Video = "Video";
        public const string Tool = "Tool";
        public const string Library = "Library";

        public static readonly string[] All = 
        { 
            Documentation, Tutorial, Course, Book, Video, Tool, Library 
        };

        /// <summary>Validates if the given type is a valid ResourceType</summary>
        public static bool IsValid(string type) => All.Contains(type);
    }

    /// <summary>
    /// Partnership relationship types
    /// </summary>
    public static class PartnerTypes
    {
        public const string Partner = "Partner";
        public const string Sponsor = "Sponsor";

        public static readonly string[] All = { Partner, Sponsor };

        /// <summary>Validates if the given type is a valid PartnerType</summary>
        public static bool IsValid(string type) => All.Contains(type);
    }

    /// <summary>
    /// Partnership tier levels
    /// </summary>
    public static class PartnerLevels
    {
        public const string Gold = "Gold";
        public const string Silver = "Silver";
        public const string Bronze = "Bronze";
        public const string Community = "Community";

        public static readonly string[] All = { Gold, Silver, Bronze, Community };

        /// <summary>Validates if the given level is a valid PartnerLevel</summary>
        public static bool IsValid(string level) => All.Contains(level);
    }

    /// <summary>
    /// Approval and status constants
    /// </summary>
    public static class ApprovalStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";

        public static readonly string[] All = { Pending, Approved, Rejected };
    }

    /// <summary>
    /// Registration status constants
    /// </summary>
    public static class RegistrationStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";

        public static readonly string[] All = { Pending, Approved, Rejected };
    }

    /// <summary>
    /// Member position constants
    /// </summary>
    public static class MemberPositions
    {
        public const string Lead = "Lead";
        public const string CoLead = "CoLead";
        public const string Organizer = "Organizer";
        public const string Volunteer = "Volunteer";

        public static readonly string[] All = { Lead, CoLead, Organizer, Volunteer };
    }
}
