using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed record RejectMemberCommand(Guid MemberId, Guid ReviewerUserId) : IRequest<TeamMember>;
