using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Api;

public class ApiStatsService : ApiServiceBase, IStatsService
{
    public ApiStatsService(HttpClient http) : base(http)
    {
    }

    public async Task<DashboardResponse?> GetDashboardAsync()
    {
        var response = await Http.GetAsync(ApiEndpoints.Stats);
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<DashboardResponse>(response);
    }
}
