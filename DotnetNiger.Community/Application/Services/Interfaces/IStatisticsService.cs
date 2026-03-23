namespace DotnetNiger.Community.Application.Services.Interfaces;

/// <summary>
/// DTO for community statistics
/// </summary>
public class CommunityStatsDto
{
    public int TotalPosts { get; set; }
    public int PublishedPosts { get; set; }
    public int TotalEvents { get; set; }
    public int TotalResources { get; set; }
    public int TotalProjects { get; set; }
    public int TotalComments { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// DTO for growth trend
/// </summary>
public class GrowthTrendDto
{
    public DateTime Date { get; set; }
    public int PostsCount { get; set; }
    public int EventsCount { get; set; }
    public int ResourcesCount { get; set; }
}

/// <summary>
/// DTO for top contributor
/// </summary>
public class TopContributorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ContributionCount { get; set; }
}

/// <summary>
/// DTO for category distribution
/// </summary>
public class CategoryDistributionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Service for community statistics and analytics
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// Gets overall community statistics
    /// </summary>
    Task<CommunityStatsDto> GetCommunityStatsAsync();

    /// <summary>
    /// Gets growth trends for the last N days
    /// </summary>
    /// <param name="days">Number of days to retrieve (default: last 30 days)</param>
    Task<IEnumerable<GrowthTrendDto>> GetGrowthTrendsAsync(int days = 30);

    /// <summary>
    /// Gets top contributors by post count
    /// </summary>
    /// <param name="limit">Maximum number of contributors to return</param>
    Task<IEnumerable<TopContributorDto>> GetTopContributorsAsync(int limit = 10);

    /// <summary>
    /// Gets category distribution with post counts
    /// </summary>
    Task<IEnumerable<CategoryDistributionDto>> GetCategoryDistributionAsync();
}
