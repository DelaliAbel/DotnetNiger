using System.Security.Claims;
using DotnetNiger.Application.Abstractions;

namespace DotnetNiger.Web.Services;

/// <summary>
/// Implémentation web de <see cref="ICurrentUserService"/>.
/// </summary>
/// <remarks>
/// L'intercepteur d'audit et les points de terminaison utilisent ce service plutôt que
/// <c>HttpContext</c> directement : la couche application reste ainsi testable sans
/// fabriquer de requête HTTP.
/// </remarks>
internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initialise le service.
    /// </summary>
    /// <param name="httpContextAccessor">Accès au contexte de la requête courante.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public Guid? UserId
        => Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Roles
        => Principal?.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray() ?? [];

    /// <inheritdoc />
    public bool IsInRole(string role) => Principal is not null && Principal.IsInRole(role);

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
}
