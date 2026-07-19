using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface IMemberDirectoryService
{
    Task<PaginatedDto<MemberDirectoryResponse>> GetAllAsync(string? query, string? country, int page = 1, int pageSize = 10);
    Task<MemberDirectoryResponse?> GetByIdAsync(Guid id);
    Task<List<TeamMemberResponse>> GetTeamMembersAsync();
}
