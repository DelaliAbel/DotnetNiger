using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiSearchService : ApiServiceBase, ISearchService
{
    public ApiSearchService(HttpClient http, ILogger<ApiSearchService> logger) : base(http, logger) { }

    public async Task<PaginatedDto<SearchResultDto>> SearchAsync(SearchQueryRequest request)
    {
        var q = new Dictionary<string, string?>
        {
            ["query"] = request.Query,
            ["type"] = request.Type,
            ["page"] = request.Page.ToString(),
            ["pageSize"] = request.PageSize.ToString()
        };
        var url = BuildUrl(ApiEndpoints.Search, q);
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new PaginatedDto<SearchResultDto>();
            }
            return await ApiResponseReader.ReadAsync<PaginatedDto<SearchResultDto>>(response)
                   ?? new PaginatedDto<SearchResultDto>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new PaginatedDto<SearchResultDto>();
        }
    }

}
