using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Exceptions;
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
[Route("api/admin/accounts")]
[Tags("Administration des comptes")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminAccountsController(
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

    /// <summary>Crée un compte depuis l'administration.</summary>
    /// <param name="request">Identité, mot de passe initial et rôles du compte.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le compte créé, avec ses rôles.</returns>
    /// <remarks>
    /// L'administration choisit le mot de passe initial et le transmet au membre par un
    /// canal qu'elle contrôle : le compte n'est donc pas présenté comme attendant une
    /// inscription en ligne. L'adresse reste à confirmer, sauf si l'administration
    /// atteste explicitement l'avoir vérifiée.
    /// </remarks>
    /// <response code="201">Compte créé.</response>
    /// <response code="400">Adresse déjà utilisée, mot de passe refusé, ou rôle inconnu.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpPost]
    [ProducesResponseType<AdminAccountResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminAccountResponse>> Creer(
        [FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var email = request.Email.Trim();

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            throw new DomainException("email_taken", "Un compte utilise déjà cette adresse.");
        }

        // L'absence de rôle ne doit pas créer un compte sans rôle : un compte qui n'en
        // porte aucun n'accède ni à l'espace membre ni à l'administration.
        var demandes = request.Roles is null or { Count: 0 }
            ? [RoleNames.Member]
            : await VerifierLesRôlesAsync([.. request.Roles], cancellationToken);

        var compte = new ApplicationUser(email);

        if (!string.IsNullOrWhiteSpace(request.FirstName) || !string.IsNullOrWhiteSpace(request.LastName))
        {
            compte.UpdateProfile(
                request.FirstName?.Trim() ?? string.Empty,
                request.LastName?.Trim() ?? string.Empty);
        }

        await VerifierAsync(userManager.CreateAsync(compte, request.Password), "invalid_password");

        await AppliquerLesRolesAsync(compte, demandes);

        if (request.ConfirmEmail)
        {
            var jeton = await userManager.GenerateEmailConfirmationTokenAsync(compte);
            await VerifierAsync(userManager.ConfirmEmailAsync(compte, jeton), "confirm_error");
        }

        return CreatedAtAction(nameof(Lire), new { id = compte.Id }, ApiProjections.ToAccount(compte, demandes));
    }

    /// <summary>Active ou désactive un compte, et gère son appartenance à l'équipe.</summary>
    /// <param name="id">Identifiant du compte.</param>
    /// <param name="request">Nouvel état du compte.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le compte mis à jour.</returns>
    /// <remarks>
    /// Un administrateur ne peut pas désactiver son propre compte : cela le verrouillerait
    /// hors de l'administration, et pourrait laisser l'application sans administrateur actif.
    /// </remarks>
    /// <response code="200">Compte mis à jour.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur, ou vient de se désactiver.</response>
    /// <response code="404">Compte introuvable.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<AdminAccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminAccountResponse>> MettreAJour(
        Guid id,
        [FromBody] UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var auteur = RequireAdministrateur(currentUser);

        if (id == auteur && !request.IsActive)
        {
            throw new DomainException("self_lockout", "Vous ne pouvez pas désactiver votre propre compte.");
        }

        var compte = await ChargerCompte(id, cancellationToken);

        if (compte is null)
        {
            return NotFound();
        }

        compte.SetActive(request.IsActive);
        compte.SetTeamMember(request.IsTeam);

        unitOfWork.Repository<ApplicationUser>().Update(compte);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToAccount(compte, await RolesDeAsync(compte)));
    }

    /// <summary>Remplace l'ensemble des rôles d'un compte.</summary>
    /// <param name="id">Identifiant du compte.</param>
    /// <param name="request">Rôles complets attendus.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le compte avec ses nouveaux rôles.</returns>
    /// <remarks>
    /// Les rôles sont remplacés, et non ajoutés : l'interface d'administration envoie la
    /// liste complète, ce qui évite qu'un retrait de rôle soit impossible à exprimer. L'appelant
    /// ne peut pas se retirer son propre rôle d'administrateur, afin de ne pas se verrouiller
    /// hors de l'administration.
    /// </remarks>
    /// <response code="200">Rôles remplacés.</response>
    /// <response code="400">Rôle inconnu, ou retrait du dernier administrateur.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur, ou retire son propre rôle.</response>
    /// <response code="404">Compte introuvable.</response>
    [HttpPut("{id:guid}/roles")]
    [ProducesResponseType<AdminAccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminAccountResponse>> RemplacerLesRoles(
        Guid id,
        [FromBody] ChangeRolesRequest request,
        CancellationToken cancellationToken)
    {
        var auteur = RequireAdministrateur(currentUser);

        var demandes = Normaliser(request.Roles);

        if (!demandes.Contains(RoleNames.Admin, StringComparer.OrdinalIgnoreCase)
            && await EstDernierAdministrateurAsync(id, demande: true, cancellationToken))
        {
            throw new DomainException("last_admin", "Impossible de retirer le dernier rôle d'administrateur actif.");
        }

        if (id == auteur && !demandes.Contains(RoleNames.Admin, StringComparer.OrdinalIgnoreCase))
        {
            throw new DomainException("self_lockout", "Vous ne pouvez pas retirer votre propre rôle d'administrateur.");
        }

        await VerifierLesRôlesAsync(demandes, cancellationToken);

        var compte = await ChargerCompte(id, cancellationToken);

        if (compte is null)
        {
            return NotFound();
        }

        await AppliquerLesRolesAsync(compte, demandes);

        return Ok(ApiProjections.ToAccount(compte, demandes));
    }

    /// <summary>Ajoute un rôle à un compte.</summary>
    /// <param name="id">Identifiant du compte.</param>
    /// <param name="role">Rôle à ajouter.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le compte avec ses nouveaux rôles.</returns>
    /// <response code="200">Rôle ajouté.</response>
    /// <response code="400">Rôle inconnu.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Compte introuvable.</response>
    [HttpPost("{id:guid}/roles")]
    [ProducesResponseType<AdminAccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminAccountResponse>> AjouterUnRole(
        Guid id,
        [FromQuery] string role,
        CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var compte = await ChargerCompte(id, cancellationToken);

        if (compte is null)
        {
            return NotFound();
        }

        var cible = (await VerifierLesRôlesAsync([role], cancellationToken))[0];

        var roles = await userManager.GetRolesAsync(compte);

        if (!roles.Contains(cible, StringComparer.OrdinalIgnoreCase))
        {
            await VerifierAsync(userManager.AddToRoleAsync(compte, cible), "role_error");
        }

        return Ok(ApiProjections.ToAccount(compte, await RolesDeAsync(compte)));
    }

    /// <summary>Retire un rôle d'un compte.</summary>
    /// <param name="id">Identifiant du compte.</param>
    /// <param name="role">Rôle à retirer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le compte sans ce rôle.</returns>
    /// <remarks>
    /// Le rôle d'administrateur n'est pas retiré s'il s'agit du dernier administrateur actif :
    /// l'application deviendrait impossible à administrer depuis son interface.
    /// </remarks>
    /// <response code="200">Rôle retiré.</response>
    /// <response code="400">Le compte ne porte pas ce rôle, ou c'est le dernier administrateur.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur, ou retire son propre rôle.</response>
    /// <response code="404">Compte introuvable.</response>
    [HttpDelete("{id:guid}/roles")]
    [ProducesResponseType<AdminAccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminAccountResponse>> RetirerUnRole(
        Guid id,
        [FromQuery] string role,
        CancellationToken cancellationToken)
    {
        var auteur = RequireAdministrateur(currentUser);

        if (id == auteur
            && string.Equals(role.Trim(), RoleNames.Admin, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("self_lockout", "Vous ne pouvez pas retirer votre propre rôle d'administrateur.");
        }

        var compte = await ChargerCompte(id, cancellationToken);

        if (compte is null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(compte);

        var cible = roles.FirstOrDefault(
            existant => string.Equals(existant, role.Trim(), StringComparison.OrdinalIgnoreCase));

        if (cible is null)
        {
            throw new DomainException("role_missing", $"Ce compte ne porte pas le rôle {role.Trim()}.");
        }

        if (string.Equals(cible, RoleNames.Admin, StringComparison.OrdinalIgnoreCase)
            && await EstDernierAdministrateurAsync(id, demande: true, cancellationToken))
        {
            throw new DomainException("last_admin", "Impossible de retirer le dernier rôle d'administrateur actif.");
        }

        await VerifierAsync(userManager.RemoveFromRoleAsync(compte, cible), "role_error");

        return Ok(ApiProjections.ToAccount(compte, await RolesDeAsync(compte)));
    }

    /// <summary>Déverrouille un compte bloqué par trop de tentatives de connexion.</summary>
    /// <param name="id">Identifiant du compte.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <remarks>
    /// Le déverrouillage remet aussi le compteur d'échecs à zéro : sans cela, le compte
    /// resterait à deux tentatives d'un nouveau blocage.
    /// </remarks>
    /// <response code="204">Compte déverrouillé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Compte introuvable.</response>
    [HttpPost("{id:guid}/unlock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Debloquer(Guid id, CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var compte = await ChargerCompte(id, cancellationToken);

        if (compte is null)
        {
            return NotFound();
        }

        await userManager.SetLockoutEndDateAsync(compte, null);
        await userManager.ResetAccessFailedCountAsync(compte);

        return NoContent();
    }

    /// <summary>Valide l'adresse électronique d'un compte.</summary>
    /// <param name="id">Identifiant du compte.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <remarks>
    /// Le compte est marqué comme vérifié sans passer par la boîte aux lettres : c'est
    /// l'administration qui atteste que l'adresse a été contrôlée par un autre canal.
    /// </remarks>
    /// <response code="204">Compte marqué comme vérifié.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    /// <response code="404">Compte introuvable ou sans adresse.</response>
    [HttpPost("{id:guid}/confirm-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmerEmail(Guid id, CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var compte = await ChargerCompte(id, cancellationToken);

        if (compte is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(compte.Email))
        {
            throw new DomainException("email_required", "Ce compte n'a pas d'adresse électronique.");
        }

        var jeton = await userManager.GenerateEmailConfirmationTokenAsync(compte);
        await VerifierAsync(userManager.ConfirmEmailAsync(compte, jeton), "confirm_error");

        return NoContent();
    }

    /// <summary>Désactive un compte et lui retire ses rôles.</summary>
    /// <param name="id">Identifiant du compte.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <remarks>
    /// Le compte n'est pas effacé : ses articles, commentaires et inscriptions sont rattachés
    /// à son identité, et une suppression physique laisserait des contenus sans auteur. La
    /// désactivation coupe l'accès et retire les rôles, et la réactivation reste possible.
    /// </remarks>
    /// <response code="204">Compte désactivé.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur, ou se désactive lui-même.</response>
    /// <response code="404">Compte introuvable.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactiver(Guid id, CancellationToken cancellationToken)
    {
        var auteur = RequireAdministrateur(currentUser);

        if (id == auteur)
        {
            throw new DomainException("self_lockout", "Vous ne pouvez pas supprimer votre propre compte.");
        }

        var compte = await ChargerCompte(id, cancellationToken);

        if (compte is null)
        {
            return NotFound();
        }

        compte.SetActive(false);
        unitOfWork.Repository<ApplicationUser>().Update(compte);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Les rôles sont retirés après l'enregistrement de la désactivation : un compte
        // inactif mais encore administrateur resterait un risque si un jeton survivait.
        await AppliquerLesRolesAsync(compte, []);

        return NoContent();
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

    /// <summary>
    /// Lit les rôles d'un compte, triés.
    /// </summary>
    /// <param name="compte">Compte concerné.</param>
    /// <returns>Noms de rôles.</returns>
    private async Task<IReadOnlyList<string>> RolesDeAsync(ApplicationUser compte) =>
        (await userManager.GetRolesAsync(compte)).Order(StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>
    /// Vérifie que des rôles existent, et retourne ceux qui sont connus.
    /// </summary>
    /// <param name="roles">Rôles demandés.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les rôles connus, dans l'ordre demandé.</returns>
    /// <exception cref="DomainException">Si un rôle n'existe pas.</exception>
    private async Task<string[]> VerifierLesRôlesAsync(string[] roles, CancellationToken cancellationToken)
    {
        var demandes = Normaliser(roles);

        if (demandes.Length == 0)
        {
            throw new DomainException("empty_roles", "Indiquez au moins un rôle.");
        }

        foreach (var role in demandes)
        {
            if (await roleManager.FindByNameAsync(role) is null)
            {
                throw new DomainException("unknown_role", $"Rôle inconnu : {role}.");
            }
        }

        return demandes;
    }

    /// <summary>
    /// Remplace les rôles d'un compte.
    /// </summary>
    /// <param name="compte">Compte concerné.</param>
    /// <param name="roles">Rôles attendus après l'opération.</param>
    private async Task AppliquerLesRolesAsync(ApplicationUser compte, string[] roles)
    {
        var actuels = await userManager.GetRolesAsync(compte);

        var aRetirer = actuels.Except(roles, StringComparer.OrdinalIgnoreCase).ToArray();
        var aAjouter = roles.Except(actuels, StringComparer.OrdinalIgnoreCase).ToArray();

        if (aRetirer.Length > 0)
        {
            await VerifierAsync(userManager.RemoveFromRolesAsync(compte, aRetirer), "role_error");
        }

        if (aAjouter.Length > 0)
        {
            await VerifierAsync(userManager.AddToRolesAsync(compte, aAjouter), "role_error");
        }
    }

    /// <summary>
    /// Indique si un compte est le dernier administrateur actif.
    /// </summary>
    /// <param name="compteId">Identifiant du compte concerné.</param>
    /// <param name="demande">La demande retire-t-elle le rôle d'administrateur ?</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns><see langword="true"/> si aucun autre administrateur actif ne subsiste.</returns>
    private async Task<bool> EstDernierAdministrateurAsync(
        Guid compteId,
        bool demande,
        CancellationToken cancellationToken)
    {
        if (!demande)
        {
            return false;
        }

        var admins = await userManager.GetUsersInRoleAsync(RoleNames.Admin);

        var maintenant = DateTimeOffset.UtcNow;

        return !admins.Any(
            admin => admin.Id != compteId
                && admin.IsActive
                && (admin.LockoutEnd is null || admin.LockoutEnd <= maintenant));
    }

    /// <summary>
    /// Nettoie et dédoublonne une liste de rôles.
    /// </summary>
    /// <param name="roles">Rôles demandés.</param>
    /// <returns>Rôles normalisés.</returns>
    private static string[] Normaliser(IEnumerable<string> roles) =>
        [.. roles
            .Select(role => role?.Trim() ?? string.Empty)
            .Where(role => role.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// Convertit un résultat d'Identity en exception du domaine.
    /// </summary>
    /// <param name="verification">Opération d'Identity à vérifier.</param>
    /// <param name="code">Code d'erreur à employer.</param>
    /// <exception cref="DomainException">Si l'opération a échoué.</exception>
    private static async Task VerifierAsync(Task<IdentityResult> verification, string code)
    {
        var resultat = await verification;

        if (!resultat.Succeeded)
        {
            throw new DomainException(
                code,
                string.Join(" ", resultat.Errors.Select(erreur => erreur.Description)));
        }
    }
}
