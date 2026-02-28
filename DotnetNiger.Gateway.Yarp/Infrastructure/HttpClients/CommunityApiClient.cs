namespace DotnetNiger.Gateway.Infrastructure.HttpClients;

/// <summary>
/// Client pour communiquer avec le service Community
/// </summary>
public class CommunityApiClient : ApiClientBase, ICommunityApiClient
{
    private const string ClusterName = "community-cluster";

    public CommunityApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CommunityApiClient> logger)
        : base(httpClient, configuration, logger)
    {
    }

    public async Task<List<PostResponse>?> GetPostsAsync(int page = 1, int pageSize = 10)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/posts?page={page}&pageSize={pageSize}";
        return await GetAsync<List<PostResponse>>(url);
    }

    public async Task<PostResponse?> GetPostByIdAsync(string postId)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/posts/{postId}";
        return await GetAsync<PostResponse>(url);
    }

    public async Task<PostResponse?> CreatePostAsync(CreatePostRequest request)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/posts";
        return await PostAsync<PostResponse>(url, request);
    }

    public async Task<List<EventResponse>?> GetEventsAsync()
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/events";
        return await GetAsync<List<EventResponse>>(url);
    }

    public async Task<EventResponse?> GetEventByIdAsync(string eventId)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/events/{eventId}";
        return await GetAsync<EventResponse>(url);
    }

    public async Task<EventResponse?> CreateEventAsync(CreateEventRequest request)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/events";
        return await PostAsync<EventResponse>(url, request);
    }

    public async Task<List<ProjectResponse>?> GetProjectsAsync()
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/projects";
        return await GetAsync<List<ProjectResponse>>(url);
    }

    public async Task<ProjectResponse?> GetProjectByIdAsync(string projectId)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/projects/{projectId}";
        return await GetAsync<ProjectResponse>(url);
    }

    public async Task<List<CategoryResponse>?> GetCategoriesAsync()
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/categories";
        return await GetAsync<List<CategoryResponse>>(url);
    }

    public async Task<List<ResourceResponse>?> GetResourcesAsync()
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/resources";
        return await GetAsync<List<ResourceResponse>>(url);
    }

    public async Task<SearchResponse?> SearchAsync(string query)
    {
        var baseAddress = GetClusterAddress(ClusterName);
        if (string.IsNullOrEmpty(baseAddress))
            return null;

        var url = $"{baseAddress}/api/search?query={Uri.EscapeDataString(query)}";
        return await GetAsync<SearchResponse>(url);
    }
}
