// using DotnetNiger.Community.Application.Services.Interfaces;
// using DotnetNiger.Community.Infrastructure.Repositories;
// using DotnetNiger.Community.Domain.Entities;

// namespace DotnetNiger.Community.Application.Services;

// public class Member : IMemberService
// {
//      private readonly IMemberRepository _memberRepository;

//     public MemberService(IMemberRepository memberRepository)
//     {
//         _memberRepository = memberRepository;
//     }

//  public async Task<IEnumerable<Member>> GetActiveMembersAsync()
//     {
//         return await _memberRepository.GetActiveMembersAsync();
//     }

// }