using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed record ApproveMemberCommand(Guid MemberId, Guid ReviewerUserId) : IRequest<ApproveMemberResult>;

public sealed record ApproveMemberResult(TeamMember Member, bool RoleAssigned);
