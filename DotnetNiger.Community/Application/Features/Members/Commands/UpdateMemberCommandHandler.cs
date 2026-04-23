using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, TeamMember?>
{
    private readonly ITeamMemberService _memberService;

    public UpdateMemberCommandHandler(ITeamMemberService memberService)
    {
        _memberService = memberService;
    }

    public async Task<TeamMember?> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var existingMember = await _memberService.GetMemberByIdAsync(request.MemberId);
        if (existingMember == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            existingMember.Name = request.Name;
        }

        if (!string.IsNullOrWhiteSpace(request.Position))
        {
            existingMember.Position = request.Position;
        }

        if (request.BioOverride != null)
        {
            existingMember.BioOverride = request.BioOverride;
        }

        if (request.Order.HasValue)
        {
            existingMember.Order = request.Order.Value;
        }

        if (request.IsPublic.HasValue)
        {
            existingMember.IsPublic = request.IsPublic.Value;
        }

        if (request.IsActive.HasValue)
        {
            existingMember.IsActive = request.IsActive.Value;
        }

        if (request.RoleDescription != null)
        {
            existingMember.RoleDescription = request.RoleDescription;
        }

        return await _memberService.UpdateMemberAsync(request.MemberId, existingMember);
    }
}
