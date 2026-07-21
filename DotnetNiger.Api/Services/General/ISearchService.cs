using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.General;

public interface ISearchService
{
    Task<SearchResultResponse> SearchAsync(SearchQueryRequest request);
}
