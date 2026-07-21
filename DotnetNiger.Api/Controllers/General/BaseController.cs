using System.Security.Claims;
using DotnetNiger.Api.Constants;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException(Messages.User.InvalidIdentity);
        return userId;
    }

    protected bool IsAdmin() =>
        User.IsInRole(RoleConstants.Admin) || User.IsInRole(RoleConstants.SuperAdmin);

    protected bool IsCollaborator() =>
        User.IsInRole(RoleConstants.Collaborator);

    protected bool HasPermission(string permission) =>
        User.HasClaim("permission", permission);

    protected string GetUserName() =>
        User.FindFirstValue("full_name") ?? User.FindFirstValue(ClaimTypes.Name) ?? "Inconnu";

    protected string GetUserAvatar() =>
        User.FindFirstValue("avatar_url") ?? string.Empty;
}
