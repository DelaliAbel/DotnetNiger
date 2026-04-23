using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Queries;

public sealed class GetAllMembersQueryHandler : IRequestHandler<GetAllMembersQuery, IEnumerable<TeamMember>>
{
    private readonly ITeamMemberService _memberService;

    public GetAllMembersQueryHandler(ITeamMemberService memberService)
    {
        _memberService = memberService;
    }

    public Task<IEnumerable<TeamMember>> Handle(GetAllMembersQuery request, CancellationToken cancellationToken)
    {
        return _memberService.GetAllMembersAsync(request.Page, request.PageSize);
    }
}
