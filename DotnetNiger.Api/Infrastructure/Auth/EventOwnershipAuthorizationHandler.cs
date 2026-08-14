using System.Security.Claims;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Api.Infrastructure.Auth;

/// <summary>
/// Gestionnaire d'autorisation basé sur la ressource : autorise le propriétaire
/// de l'événement ainsi que l'équipe (Admin, SuperAdmin, Collaborateur).
/// Enregistré en scoped car il dépend de <see cref="DotnetNigerDbContext"/>.
/// </summary>
public class EventOwnershipAuthorizationHandler : AuthorizationHandler<EventOwnershipRequirement>
{
    private readonly DotnetNigerDbContext _db;

    public EventOwnershipAuthorizationHandler(DotnetNigerDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EventOwnershipRequirement requirement)
    {
        if (context.Resource is not Guid eventId)
            return;

        if (context.User.IsInRole(RoleConstants.Admin)
            || context.User.IsInRole(RoleConstants.SuperAdmin)
            || context.User.IsInRole(RoleConstants.Collaborator))
        {
            context.Succeed(requirement);
            return;
        }

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return;

        var isOwner = await _db.Events
            .AsNoTracking()
            .AnyAsync(e => e.Id == eventId && e.CreatedBy == userId);

        if (isOwner)
            context.Succeed(requirement);
    }
}
