using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface ISearchService
{
    Task<PaginatedDto<SearchResultDto>> SearchAsync(SearchQueryRequest request);
}
