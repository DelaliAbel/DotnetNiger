using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services;

public interface ISearchService
{
    Task<SearchResultResponse> SearchAsync(SearchQueryRequest request);
}
