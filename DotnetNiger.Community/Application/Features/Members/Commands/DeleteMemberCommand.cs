using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed record DeleteMemberCommand(Guid MemberId) : IRequest<bool>;
