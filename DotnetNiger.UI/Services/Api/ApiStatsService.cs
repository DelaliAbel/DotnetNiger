using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiStatsService : ApiServiceBase, IStatsService
{
    public ApiStatsService(HttpClient http, ILogger<ApiStatsService> logger) : base(http, logger) { }

    public async Task<DashboardResponse?> GetDashboardAsync()
    {
        var url = ApiEndpoints.Stats;
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<DashboardResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }
}
