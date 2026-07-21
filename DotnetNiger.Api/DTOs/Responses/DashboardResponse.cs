namespace DotnetNiger.Api.DTOs.Responses;

public record DashboardStats(
    int TotalUsers,
    int TotalRoles,
    int TotalEvents,
    int TotalPosts,
    int TotalResources,
    int TotalProjects,
    int TotalMembers,
    int PendingEvents,
    int PendingPosts);
