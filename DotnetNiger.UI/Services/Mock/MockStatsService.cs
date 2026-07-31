using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Threading;

namespace DotnetNiger.UI.Services.Mock;

public class MockStatsService : IStatsService
{
    public Task<DashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<DashboardResponse?>(new DashboardResponse
        {
            PostsCount = 24,
            PublishedPostsCount = 18,
            DraftPostsCount = 6,
            EventsCount = 12,
            UpcomingEventsCount = 3,
            PastEventsCount = 8,
            PendingEventsCount = 1,
            ResourcesCount = 45,
            TotalResourceViews = 1250,
            MembersCount = 156,
            ActiveNewsletterCount = 89,
            CommentsCount = 67,
            ProjectsCount = 15,
            PartnersCount = 8,
            PendingCertificatesCount = 5
        });
    }
}
