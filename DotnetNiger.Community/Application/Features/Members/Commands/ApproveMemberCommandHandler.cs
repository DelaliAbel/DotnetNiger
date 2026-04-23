using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed class ApproveMemberCommandHandler : IRequestHandler<ApproveMemberCommand, ApproveMemberResult>
{
    private readonly ITeamMemberService _memberService;
    private readonly IIdentityApiClient _identityApiClient;

    public ApproveMemberCommandHandler(ITeamMemberService memberService, IIdentityApiClient identityApiClient)
    {
        _memberService = memberService;
        _identityApiClient = identityApiClient;
    }

    public async Task<ApproveMemberResult> Handle(ApproveMemberCommand request, CancellationToken cancellationToken)
    {
        var approved = await _memberService.ApproveMemberAsync(request.MemberId, request.ReviewerUserId);
        var roleAssigned = await _identityApiClient.AssignMemberRoleAsync(approved.UserId, cancellationToken);
        return new ApproveMemberResult(approved, roleAssigned);
    }
}
