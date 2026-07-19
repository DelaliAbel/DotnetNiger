namespace DotnetNiger.Client.Models.Responses;

public class DashboardResponse
{
    public int PostsCount { get; set; }
    public int PublishedPostsCount { get; set; }
    public int DraftPostsCount { get; set; }
    public int EventsCount { get; set; }
    public int UpcomingEventsCount { get; set; }
    public int PastEventsCount { get; set; }
    public int PendingEventsCount { get; set; }
    public int ResourcesCount { get; set; }
    public int TotalResourceViews { get; set; }
    public int MembersCount { get; set; }
    public int ActiveNewsletterCount { get; set; }
    public int CommentsCount { get; set; }
    public int ProjectsCount { get; set; }
    public int PartnersCount { get; set; }
    public int PendingCertificatesCount { get; set; }
}
