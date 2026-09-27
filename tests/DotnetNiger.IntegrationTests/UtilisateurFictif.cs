using DotnetNiger.Application.Abstractions;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Implémentation de <see cref="ICurrentUserService"/> pour les tests, sans
/// <c>HttpContext</c>.
/// </summary>
internal sealed class UtilisateurFictif : ICurrentUserService
{
    private readonly HashSet<string> _roles;

    private UtilisateurFictif(Guid? userId, params string[] roles)
    {
        UserId = userId;
        _roles = [.. roles];
    }

    public static UtilisateurFictif Anonyme => new(null);

    public static UtilisateurFictif Membre(Guid id) => new(id, "Member");

    public static UtilisateurFictif Administrateur(Guid id) => new(id, "Member", "Admin");

    public bool IsAuthenticated => UserId.HasValue;

    public Guid? UserId { get; }

    public IReadOnlyCollection<string> Roles => _roles;

    public bool IsInRole(string role) => _roles.Contains(role);
}
