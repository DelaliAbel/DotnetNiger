using DotnetNiger.Infrastructure.Identity;

namespace DotnetNiger.Web.Controller.Contracts;

internal static partial class ApiProjections
{
    /// <summary>
    /// Projette un compte pour l'administration.
    /// </summary>
    /// <param name="user">Compte à projeter.</param>
    /// <param name="roles">Rôles attribués, résolus hors requête.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static AdminAccountResponse ToAccount(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(
            user.Id,
            user.Email,
            user.Slug,
            user.FullName,
            user.IsActive,
            user.IsTeam,
            user.EmailConfirmed,
            roles,
            user.LockoutEnd,
            user.CreatedAtUtc);

}
