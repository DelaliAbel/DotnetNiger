using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Rôles portés par un compte.
/// </summary>
/// <remarks>
/// Toutes les écritures convergent vers <c>AppliquerLesRolesAsync</c>, qui ne fait que la
/// différence entre les rôles actuels et les rôles attendus. Une exception : l'administrateur
/// ne peut pas se retirer son propre rôle, ni être le dernier à le porter, sans quoi
/// l'application deviendrait impossible à administrer depuis son interface.
/// </remarks>
public sealed partial class AdminAccountsController
{
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
}