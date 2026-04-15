using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Application.Abstractions.Persistence;

namespace DotnetNiger.Community.Application.Services;

/// <summary>
/// Service for retrieving and computing community statistics and analytics
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly IPostPersistence _postRepository;
    private readonly IEventPersistence _eventRepository;
    private readonly IResourcePersistence _resourceRepository;
    private readonly IProjectPersistence _projectRepository;
    private readonly ICategoryPersistence _categoryRepository;
    private readonly ICommentPersistence _commentRepository;

    public StatisticsService(
        IPostPersistence postRepository,
        IEventPersistence eventRepository,
        IResourcePersistence resourceRepository,
        IProjectPersistence projectRepository,
        ICategoryPersistence categoryRepository,
        ICommentPersistence commentRepository)
    {
        _postRepository = postRepository;
        _eventRepository = eventRepository;
        _resourceRepository = resourceRepository;
        _projectRepository = projectRepository;
        _categoryRepository = categoryRepository;
        _commentRepository = commentRepository;
    }

    public async Task<CommunityStatsDto> GetCommunityStatsAsync()
    {
        var totalPostsTask = _postRepository.CountAsync();
        var publishedPostsTask = _postRepository.CountAsync(post => post.IsPublished);
        var totalEventsTask = _eventRepository.CountAsync();
        var totalResourcesTask = _resourceRepository.CountAsync();
        var totalProjectsTask = _projectRepository.CountAsync();
        var totalCommentsTask = _commentRepository.CountAsync();

        await Task.WhenAll(
            totalPostsTask,
            publishedPostsTask,
            totalEventsTask,
            totalResourcesTask,
            totalProjectsTask,
            totalCommentsTask);

        return new CommunityStatsDto
        {
            TotalPosts = totalPostsTask.Result,
            PublishedPosts = publishedPostsTask.Result,
            TotalEvents = totalEventsTask.Result,
            TotalResources = totalResourcesTask.Result,
            TotalProjects = totalProjectsTask.Result,
            TotalComments = totalCommentsTask.Result,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<GrowthTrendDto>> GetGrowthTrendsAsync(int days = 30)
    {
        var posts = await _postRepository.GetAllAsync();
        var events = await _eventRepository.GetAllAsync();
        var resources = await _resourceRepository.GetAllAsync();

        var startDate = DateTime.UtcNow.AddDays(-days);
        var trends = new Dictionary<DateTime, GrowthTrendDto>();

        // Group posts by date
        foreach (var post in posts.Where(p => p.CreatedAt >= startDate))
        {
            var dateKey = post.CreatedAt.Date;
            if (!trends.ContainsKey(dateKey))
            {
                trends[dateKey] = new GrowthTrendDto { Date = dateKey };
            }
            trends[dateKey].PostsCount++;
        }

        // Group events by date
        foreach (var @event in events.Where(e => e.CreatedAt >= startDate))
        {
            var dateKey = @event.CreatedAt.Date;
            if (!trends.ContainsKey(dateKey))
            {
                trends[dateKey] = new GrowthTrendDto { Date = dateKey };
            }
            trends[dateKey].EventsCount++;
        }

        // Group resources by date
        foreach (var resource in resources.Where(r => r.CreatedAt >= startDate))
        {
            var dateKey = resource.CreatedAt.Date;
            if (!trends.ContainsKey(dateKey))
            {
                trends[dateKey] = new GrowthTrendDto { Date = dateKey };
            }
            trends[dateKey].ResourcesCount++;
        }

        return trends
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value);
    }

    public async Task<IEnumerable<TopContributorDto>> GetTopContributorsAsync(int limit = 10)
    {
        var posts = await _postRepository.GetAllAsync();
        var comments = await _commentRepository.GetAllAsync();

        // Group by creator (assuming posts and comments have CreatedBy or similar information)
        // For now, we'll create a simple aggregation based on posts
        var contributors = posts
            .GroupBy(p => new { p.Id })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.Id)
            .Take(limit)
            .Select((group, index) => new TopContributorDto
            {
                Id = group.Key.Id,
                Name = $"Contributor {index + 1}",
                ContributionCount = group.Count()
            });

        return contributors;
    }

    public async Task<IEnumerable<CategoryDistributionDto>> GetCategoryDistributionAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var totalPublishedPosts = await _postRepository.CountAsync(post => post.IsPublished);

        var distribution = categories
            .Select(category => new CategoryDistributionDto
            {
                Id = category.Id,
                Name = category.Name,
                PostCount = category.PostCount,
                Percentage = totalPublishedPosts > 0
                    ? Math.Round((double)category.PostCount / totalPublishedPosts * 100, 2)
                    : 0
            })
            .OrderByDescending(d => d.PostCount);

        return distribution;
    }
}
