using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Domain.Enums;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, TeamMember>
{
    private readonly ITeamMemberService _memberService;

    public CreateMemberCommandHandler(ITeamMemberService memberService)
    {
        _memberService = memberService;
    }

    public Task<TeamMember> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = new TeamMember
        {
            UserId = request.UserId,
            Name = request.Name,
            Position = request.Position,
            BioOverride = request.BioOverride ?? string.Empty,
            Order = request.Order,
            IsPublic = request.IsPublic,
            IsActive = false,
            MembershipStatus = ApprovalStatus.Pending,
            RoleDescription = request.RoleDescription ?? string.Empty,
            JoinedAt = DateTime.UtcNow
        };

        return _memberService.CreateMemberAsync(member);
    }
}
