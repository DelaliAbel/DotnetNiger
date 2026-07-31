using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface IStatsService
{
    Task<DashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default);
}
