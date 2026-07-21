using DotnetNiger.UI.Models.Responses;

namespace DotnetNiger.UI.Services.Contracts;

public interface IStatsService
{
    Task<DashboardResponse?> GetDashboardAsync();
}
