using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Queries;

public sealed class GetActiveMembersQueryHandler : IRequestHandler<GetActiveMembersQuery, IEnumerable<TeamMember>>
{
    private readonly ITeamMemberService _memberService;

    public GetActiveMembersQueryHandler(ITeamMemberService memberService)
    {
        _memberService = memberService;
    }

    public Task<IEnumerable<TeamMember>> Handle(GetActiveMembersQuery request, CancellationToken cancellationToken)
    {
        return _memberService.GetActiveMembersAsync();
    }
}
