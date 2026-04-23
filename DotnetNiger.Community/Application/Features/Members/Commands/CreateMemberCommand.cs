using DotnetNiger.Community.Domain.Entities;
using MediatR;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed record CreateMemberCommand(
    Guid UserId,
    string Name,
    string Position,
    string? BioOverride,
    int Order,
    bool IsPublic,
    string? RoleDescription) : IRequest<TeamMember>;
