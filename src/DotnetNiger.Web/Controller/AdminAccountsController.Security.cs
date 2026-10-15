using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// État de connexion d'un compte : blocage après échecs répétés et validation d'adresse.
/// </summary>
/// <remarks>
/// Ces deux opérations ne changent ni le contenu du compte ni ses droits : elles rétablissent
/// un accès que la sécurité avait refusé, ou attestent d'un contrôle fait ailleurs.
/// </remarks>
public sealed partial class AdminAccountsController
{
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