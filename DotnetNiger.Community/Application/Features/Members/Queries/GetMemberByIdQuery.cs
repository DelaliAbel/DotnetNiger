using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Queries;

public sealed record GetMemberByIdQuery(Guid MemberId) : IRequest<TeamMember?>;
