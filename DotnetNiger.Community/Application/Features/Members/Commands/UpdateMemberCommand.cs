using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed record UpdateMemberCommand(
    Guid MemberId,
    string? Name,
    string? Position,
    string? BioOverride,
    int? Order,
    bool? IsPublic,
    bool? IsActive,
    string? RoleDescription) : IRequest<TeamMember?>;
