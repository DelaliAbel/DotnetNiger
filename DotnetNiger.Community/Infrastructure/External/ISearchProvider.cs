using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Infrastructure.External;

public interface ISearchProvider
{
    Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
