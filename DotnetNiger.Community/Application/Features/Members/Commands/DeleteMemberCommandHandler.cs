using DotnetNiger.Community.Application.Services.Interfaces;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, bool>
{
    private readonly ITeamMemberService _memberService;

    public DeleteMemberCommandHandler(ITeamMemberService memberService)
    {
        _memberService = memberService;
    }

    public Task<bool> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        return _memberService.DeleteMemberAsync(request.MemberId);
    }
}
