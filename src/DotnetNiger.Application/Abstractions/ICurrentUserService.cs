namespace DotnetNiger.Application.Abstractions;

/// <summary>
/// Expose l'utilisateur courant à la couche application, sans dépendance à
/// <c>HttpContext</c>.
/// </summary>
/// <remarks>
/// Cette interface rend l'application testable : un test fournit une implémentation
/// fictive au lieu de fabriquer une requête HTTP.
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>
    /// Obtient une valeur indiquant si la requête est authentifiée.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Obtient l'identifiant de l'utilisateur courant, ou <see langword="null"/> si la requête est anonyme.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Obtient les rôles de l'utilisateur courant.
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>
    /// Indique si l'utilisateur courant possède le rôle demandé.
    /// </summary>
    /// <param name="role">Nom du rôle.</param>
    /// <returns><see langword="true"/> si le rôle est possessed.</returns>
    bool IsInRole(string role);
}
