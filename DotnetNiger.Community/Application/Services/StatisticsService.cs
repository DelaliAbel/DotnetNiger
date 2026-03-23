using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Repositories;

namespace DotnetNiger.Community.Application.Services;

/// <summary>
/// Service for retrieving and computing community statistics and analytics
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly IPostRepository _postRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICommentRepository _commentRepository;

    public StatisticsService(
        IPostRepository postRepository,
        IEventRepository eventRepository,
        IResourceRepository resourceRepository,
        IProjectRepository projectRepository,
        ICategoryRepository categoryRepository,
        ICommentRepository commentRepository)
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
        var posts = await _postRepository.GetAllAsync();
        var events = await _eventRepository.GetAllAsync();
        var resources = await _resourceRepository.GetAllAsync();
        var projects = await _projectRepository.GetAllAsync();
        var comments = await _commentRepository.GetAllAsync();

        return new CommunityStatsDto
        {
            TotalPosts = posts.Count(),
            PublishedPosts = posts.Count(p => p.IsPublished),
            TotalEvents = events.Count(),
            TotalResources = resources.Count(),
            TotalProjects = projects.Count(),
            TotalComments = comments.Count(),
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
        var posts = await _postRepository.GetAllAsync();

        var totalPublishedPosts = posts.Count(p => p.IsPublished);

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
