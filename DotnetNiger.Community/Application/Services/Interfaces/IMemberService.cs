using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface IMemberService
{
    Task<IEnumerable<Member>> GetActiveMembersAsync();
    // Task<Member?> GetMemberByIdAsync(Guid id);
    // Task<Member> CreateMemberAsync(Member Member);
    // Task<Member> UpdateMemberAsync(Member Member);
    // Task<bool> DeleteMemberAsync(Guid id);
}