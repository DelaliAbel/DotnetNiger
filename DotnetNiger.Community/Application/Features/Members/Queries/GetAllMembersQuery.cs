using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Queries;

public sealed record GetAllMembersQuery(int Page, int PageSize) : IRequest<IEnumerable<TeamMember>>;
