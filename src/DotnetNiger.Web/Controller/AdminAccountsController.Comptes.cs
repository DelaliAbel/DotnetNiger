using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Création, mise à jour et désactivation d'un compte.
/// </summary>
/// <remarks>
/// Un compte n'est jamais effacé : ses contenus sont rattachés à son identité. La
/// désactivation coupe l'accès et retire les rôles, et la réactivation reste possible.
/// </remarks>
public sealed partial class AdminAccountsController
{
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
                request.LastName?.Trim() ?? string.Empty,
                request.Profession,
                request.PhotoUrl,
                request.GitHubUsername,
                request.LinkedInUrl,
                request.XUrl);
        }

        // L'appartenance à l'équipe est posée même sans nom : un compte créé vide puis
        // complété par l'administration doit déjà figurer dans l'équipe.
        compte.SetTeamMember(request.IsTeam);

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

        // Le profil n'est touché que sur les champs fournis : sans cela, un simple changement
        // d'état — désactiver un compte qui se fait voler — effacerait le nom, la fonction
        // et la photo que l'administration avait saisis. Chaque champ fourni remplace
        // l'ancien ; un champ omis le conserve ; une chaîne vide l'efface.
        if (request.FirstName is not null || request.LastName is not null)
        {
            compte.UpdateProfile(
               (request.FirstName ?? compte.FirstName).Trim(),
                (request.LastName ?? compte.LastName).Trim(),
                request.Profession ?? compte.Profession,
                request.PhotoUrl ?? compte.PhotoUrl,
                request.GitHubUsername ?? compte.GitHubUsername,
                request.LinkedInUrl ?? compte.LinkedInUrl,
                request.XUrl ?? compte.XUrl);
        }

        unitOfWork.Repository<ApplicationUser>().Update(compte);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToAccount(compte, await RolesDeAsync(compte)));
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
}