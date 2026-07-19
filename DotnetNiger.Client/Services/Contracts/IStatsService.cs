using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface IStatsService
{
    Task<DashboardResponse?> GetDashboardAsync();
}
