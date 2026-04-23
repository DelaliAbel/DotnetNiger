using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Queries;

public sealed class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, TeamMember?>
{
    private readonly ITeamMemberService _memberService;

    public GetMemberByIdQueryHandler(ITeamMemberService memberService)
    {
        _memberService = memberService;
    }

    public Task<TeamMember?> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        return _memberService.GetMemberByIdAsync(request.MemberId);
    }
}
