using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Infrastructure.External;

public class ElasticsearchProvider : ISearchProvider
{
    private readonly ILogger<ElasticsearchProvider> _logger;

    public ElasticsearchProvider(ILogger<ElasticsearchProvider> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ElasticsearchProvider not configured, returning empty result for query '{Query}'", query);
        return Task.FromResult<IReadOnlyList<SearchResultDto>>(Array.Empty<SearchResultDto>());
    }
}
