using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface ITeamMemberService
{
    Task<IEnumerable<TeamMember>> GetAllMembersAsync(int page = 1, int pageSize = 10);
    Task<IEnumerable<TeamMember>> GetActiveMembersAsync();
    Task<TeamMember?> GetMemberByIdAsync(Guid id);
    Task<TeamMember> CreateMemberAsync(TeamMember member);
    Task<TeamMember> UpdateMemberAsync(Guid id, TeamMember member);
    Task<bool> DeleteMemberAsync(Guid id);
}
