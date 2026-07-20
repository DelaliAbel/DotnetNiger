using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.General;

public interface ISearchService
{
    Task<SearchResultResponse> SearchAsync(SearchQueryRequest request);
}
