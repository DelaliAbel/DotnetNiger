using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface ITeamMemberPersistence : ICrudPersistence<TeamMember>
{
    Task<IEnumerable<TeamMember>> GetActiveMembersAsync();
}
