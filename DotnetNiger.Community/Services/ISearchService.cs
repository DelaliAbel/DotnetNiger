using DotnetNiger.Community.Dtos.Requests;
using DotnetNiger.Community.Dtos.Responses;

namespace DotnetNiger.Community.Services;

public interface ISearchService
{
    Task<PaginatedResponse<SearchResultResponse>> SearchAsync(SearchQueryRequest request);
}
