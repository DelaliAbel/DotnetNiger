using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Api;

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
