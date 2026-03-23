using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface IMemberService
{
    Task<IEnumerable<Member>> GetAllMembersAsync(int page = 1, int pageSize = 10);
    Task<IEnumerable<Member>> GetActiveMembersAsync();
    Task<Member?> GetMemberByIdAsync(Guid id);
    Task<Member> CreateMemberAsync(Member member);
    Task<Member> UpdateMemberAsync(Guid id, Member member);
    Task<bool> DeleteMemberAsync(Guid id);
}