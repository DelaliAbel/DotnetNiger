using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Un compte, tel que l'administration le voit.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Courriel">Adresse de connexion.</param>
/// <param name="Prenom">Prénom.</param>
/// <param name="Nom">Nom.</param>
/// <param name="NomComplet">Prénom et nom réunis, pour l'affichage.</param>
/// <param name="PhotoUrl">Photo du profil, ou <see langword="null"/>.</param>
/// <param name="EstActif">Vrai si le compte peut se connecter.</param>
/// <param name="EstEquipe">Membre de l'équipe organisatrice.</param>
/// <param name="EmailConfirme">Vrai si l'adresse a été contrôlée.</param>
/// <param name="Roles">Rôles du compte.</param>
/// <param name="BloqueJusquAu">Fin de verrouillage, ou <see langword="null"/>.</param>
/// <param name="InscritLe">Date d'inscription (UTC).</param>
public sealed record LigneMembreAdmin(
    Guid Id,
    string? Courriel,
    string Prenom,
    string Nom,
    string NomComplet,
    string? PhotoUrl,
    bool EstActif,
    bool EstEquipe,
    bool EmailConfirme,
    IReadOnlyList<string> Roles,
    DateTimeOffset? BloqueJusquAu,
    DateTime InscritLe);

/// <summary>Tous les comptes, lus pour la page Membres.</summary>
/// <param name="Membres">Comptes, du plus récemment inscrit au plus ancien.</param>
/// <param name="RolesDisponibles">Rôles attribuables, pour l'éditeur.</param>
public sealed record MembresVue(
    IReadOnlyList<LigneMembreAdmin> Membres,
    IReadOnlyList<string> RolesDisponibles);

/// <summary>
/// Lecture des comptes pour l'administration. Les écritures passent par les points
/// d'extermination de l'API.
/// </summary>
/// <remarks>
/// Aucun état n'est filtré : un compte désactivé doit rester trouvable, sinon il deviendrait
/// impossible à réactiver et son nom disparaîtrait des signalements qu'il a produits.
/// </remarks>
public sealed class ServiceMembres(
    IUnitOfWork uniteDeTravail,
    UserManager<ApplicationUser> comptes,
    RoleManager<ApplicationRole> roles)
{
    private Task<MembresVue>? _lecture;

    /// <summary>Lit les comptes, une seule fois par changement.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les comptes de la communauté.</returns>
    public Task<MembresVue> LireAsync(CancellationToken cancellationToken = default)
        => (_lecture ??= LireInterneAsync(cancellationToken));

    /// <summary>Relit la liste à la prochaine demande, après une écriture.</summary>
    public void Oublie() => _lecture = null;

    private async Task<MembresVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        var membres = await uniteDeTravail.ReadRepository<ApplicationUser>().Query
            .AsNoTracking()
            .OrderByDescending(membre => membre.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var attribues = await RolesDeAsync(membres.Select(membre => membre.Id).ToArray());

        return new MembresVue(
            [.. membres.Select(membre => new LigneMembreAdmin(
                membre.Id,
                membre.Email,
                membre.FirstName,
                membre.LastName,
                membre.FullName,
                membre.PhotoUrl,
                membre.IsActive,
                membre.IsTeam,
                membre.EmailConfirmed,
                attribues.TryGetValue(membre.Id, out var sesRoles) ? sesRoles : [],
                membre.LockoutEnd,
                membre.CreatedAtUtc))],
            [.. RolesDisponibles()]);
    }

    /// <summary>
    /// Noms des rôles attribuables, pour l'éditeur de compte.
    /// </summary>
    /// <returns>Les noms de rôles, triés.</returns>
    /// <remarks>
    /// Le tri se fait en mémoire, après la lecture : <c>RoleManager.Roles</c> est une requête,
    /// et un <c>OrderBy</c> assorti d'un comparateur n'y est pas traduisible — il ferait
    /// échouer la page entière. Deux rôles se trient aussi bien en mémoire.
    /// </remarks>
    private IReadOnlyList<string> RolesDisponibles()
    {
        // `RoleManager.Roles` est une requête, pas une liste : tout ce qui suit le tri doit
        // rester en mémoire. Un `OrderBy` assorti d'un comparateur est precisely le cas que
        // le fournisseur SQL refuse de traduire, et son refus fait échouer toute la page.
        var noms = roles.Roles.ToList().Select(role => role.Name ?? string.Empty);

        return [.. noms.Where(nom => nom.Length > 0).OrderBy(nom => nom, StringComparer.Ordinal)];
    }

    /// <summary>
    /// Résout les rôles de plusieurs comptes en une requête par rôle.
    /// </summary>
    /// <param name="ids">Identifiants de comptes.</param>
    /// <returns>Correspondance identifiant vers noms de rôles.</returns>
    /// <remarks>
    /// La résolution se fait par rôle et non par compte : l'application en compte deux,
    /// alors qu'une page d'administration peut en afficher bien davantage.
    /// </remarks>
    private async Task<Dictionary<Guid, IReadOnlyList<string>>> RolesDeAsync(Guid[] ids)
    {
        if (ids.Length is 0)
        {
            return [];
        }

        var wanted = ids.ToHashSet();
        var attribues = new Dictionary<Guid, IReadOnlyList<string>>();

        foreach (var role in roles.Roles)
        {
            var membres = await comptes.GetUsersInRoleAsync(role.Name!);

            foreach (var membre in membres.Where(membre => wanted.Contains(membre.Id)))
            {
                if (attribues.TryGetValue(membre.Id, out var existant))
                {
                    attribues[membre.Id] = [.. existant, role.Name!];
                }
                else
                {
                    attribues[membre.Id] = [role.Name!];
                }
            }
        }

        return attribues;
    }
}