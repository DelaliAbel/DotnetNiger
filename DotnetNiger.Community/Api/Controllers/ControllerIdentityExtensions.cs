using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

internal static class ControllerIdentityExtensions
{
    internal static bool TryGetCurrentUserId(this ControllerBase controller, out Guid userId)
    {
        var claimValue = controller.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? controller.User.FindFirstValue("sub")
            ?? controller.User.FindFirstValue("user_id");

        if (Guid.TryParse(claimValue, out userId))
        {
            return true;
        }

        var headerValue = controller.Request.Headers["X-User-Id"].ToString();
        return Guid.TryParse(headerValue, out userId);
    }
}
