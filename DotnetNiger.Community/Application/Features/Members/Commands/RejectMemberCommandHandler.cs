using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed class RejectMemberCommandHandler : IRequestHandler<RejectMemberCommand, TeamMember>
{
    private readonly ITeamMemberService _memberService;

    public RejectMemberCommandHandler(ITeamMemberService memberService)
    {
        _memberService = memberService;
    }

    public Task<TeamMember> Handle(RejectMemberCommand request, CancellationToken cancellationToken)
    {
        return _memberService.RejectMemberAsync(request.MemberId, request.ReviewerUserId);
    }
}
