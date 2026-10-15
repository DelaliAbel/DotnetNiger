using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Gestion des comptes, réservée à l'administration.
/// </summary>
/// <remarks>
/// Ce fichier ne porte que la lecture des comptes et la résolution de leurs rôles pour une
/// page entière. Les écritures sont réparties dans les parties voisines : comptes d'un côté,
/// rôles et état de connexion de l'autre.
/// </remarks>
[Route("api/admin/accounts")]
[Tags("Administration des comptes")]
[Authorize(Roles = RoleNames.Admin)]
public sealed partial class AdminAccountsController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : ApiControllerBase
{
    /// <summary>Liste les comptes, actifs ou non.</summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="trierPar">Champ de tri : <c>created</c>, <c>nom</c> ou <c>email</c>.</param>
    /// <param name="ascendant">Trie par ordre croissant.</param>
    /// <param name="inactifsSeulement">Restreint aux comptes désactivés.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Une page de comptes.</returns>
    /// <response code="200">Comptes listés.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpGet]
    [ProducesResponseType<PageResponse<AdminAccountResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PageResponse<AdminAccountResponse>>> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? trierPar = "created",
        [FromQuery] bool ascendant = false,
        [FromQuery] bool inactifsSeulement = false,
        CancellationToken cancellationToken = default)
    {
        RequireAdministrateur(currentUser);

        var demande = new PageRequest(page, pageSize);

        var comptes = unitOfWork.ReadRepository<ApplicationUser>().Query
            .Where(user => !inactifsSeulement || !user.IsActive);

        comptes = (trierPar?.ToLowerInvariant()) switch
        {
            "nom" => ascendant
                ? comptes.OrderBy(user => user.LastName).ThenBy(user => user.FirstName)
                : comptes.OrderByDescending(user => user.LastName).ThenByDescending(user => user.FirstName),
            "email" => ascendant
                ? comptes.OrderBy(user => user.Email)
                : comptes.OrderByDescending(user => user.Email),
            _ => ascendant
                ? comptes.OrderBy(user => user.CreatedAtUtc)
                : comptes.OrderByDescending(user => user.CreatedAtUtc),
        };

        var total = await comptes.CountAsync(cancellationToken);

        if (total == 0)
        {
            return ApiProjections.ToPage(
                PagedResultFactory.Empty<ApplicationUser>(demande),
                compte => ApiProjections.ToAccount(compte, []));
        }

        var items = await comptes
            .Skip(demande.Skip)
            .Take(demande.PageSize)
            .ToListAsync(cancellationToken);

        // Les rôles sont résolus par rôle, et non par compte : l'application en compte quelques
        // dizaines, alors qu'une page d'administration peut en lister davantage.
        var roles = await RolesAsync(items.Select(compte => compte.Id).ToArray(), cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<ApplicationUser>(items, demande.PageNumber, demande.PageSize, total),
            compte => ApiProjections.ToAccount(compte, roles.GetValueOrDefault(compte.Id, [])));
    }

    /// <summary>Lit un compte, avec ses rôles.</summary>
    /// <param name="id">Identifiant du compte.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le compte demandé.</returns>
    /// <response code="200">Compte trouvé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Compte introuvable.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AdminAccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminAccountResponse>> Lire(Guid id, CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var compte = await ChargerCompte(id, cancellationToken);

        if (compte is null)
        {
            return NotFound();
        }

        return Ok(ApiProjections.ToAccount(compte, await RolesDeAsync(compte)));
    }

    private Task<ApplicationUser?> ChargerCompte(Guid id, CancellationToken cancellationToken) =>
        unitOfWork.ReadRepository<ApplicationUser>().Query
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    /// <summary>
    /// Résout les rôles de plusieurs comptes en une requête par rôle.
    /// </summary>
    /// <param name="ids">Identifiants de comptes.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Correspondance identifiant vers noms de rôles.</returns>
    private async Task<Dictionary<Guid, IReadOnlyList<string>>> RolesAsync(
        Guid[] ids,
        CancellationToken cancellationToken)
    {
        if (ids.Length == 0)
        {
            return [];
        }

        var wanted = ids.ToHashSet();
        var resultat = new Dictionary<Guid, IReadOnlyList<string>>();

        foreach (var role in roleManager.Roles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var membres = await userManager.GetUsersInRoleAsync(role.Name!);
            var concernes = membres.Where(membre => wanted.Contains(membre.Id)).ToArray();

            if (concernes.Length == 0)
            {
                continue;
            }

            foreach (var membre in concernes)
            {
                if (resultat.TryGetValue(membre.Id, out var existant))
                {
                    resultat[membre.Id] = [.. existant, role.Name!];
                }
                else
                {
                    resultat[membre.Id] = [role.Name!];
                }
            }
        }

        return resultat;
    }
}