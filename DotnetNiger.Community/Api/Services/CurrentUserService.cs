using System.Security.Claims;

namespace DotnetNiger.Community.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetRequiredUserId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            throw new UnauthorizedAccessException("Contexte HTTP indisponible.");
        }

        var userIdClaim = context.User.FindFirstValue("user")
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.Request.Headers["X-User-Id"].FirstOrDefault();

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Claim user ou header X-User-Id requis.");
        }

        return userId;
    }
}
