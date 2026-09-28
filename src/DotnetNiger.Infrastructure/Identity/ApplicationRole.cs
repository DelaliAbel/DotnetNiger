using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Infrastructure.Identity;

/// <summary>
/// Rôle ASP.NET Identity de DotnetNiger.
/// </summary>
/// <remarks>
/// Deux rôles seulement existent : <c>Admin</c> et <c>Member</c>. Il n'y a pas de rôle de
/// modérateur : le traitement des signalements revient aux administrateurs.
/// </remarks>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Initialise un rôle sans nom, réservé à Entity Framework Core.
    /// </summary>
    public ApplicationRole()
    {
    }

    /// <summary>
    /// Initialise un rôle.
    /// </summary>
    /// <param name="name">Nom du rôle.</param>
    /// <param name="description">Description affichée dans le tableau de bord.</param>
    public ApplicationRole(string name, string? description = null)
        : base(name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Description = description;
    }

    /// <summary>
    /// Obtient la description du rôle, affichée dans le tableau de bord.
    /// </summary>
    public string? Description { get; private set; }
}
