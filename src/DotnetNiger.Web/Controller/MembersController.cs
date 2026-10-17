using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
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
/// Profils publics de membres.
/// </summary>
/// <remarks>
/// Le profil public ne contient que ce qu'un visiteur peut voir : ni adresse de courriel, ni
/// identifiant de compte externe, ni rôle. Un compte inactif n'est pas listé, même s'il reste
/// en base pour l'audit.
/// </remarks>
[Route("api/members")]
[Tags("Membres")]
public sealed class MembersController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IAccountDeletionService accountDeletion) : ApiControllerBase
{
    /// <summary>Liste les profils publics.</summary>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="team">Ne garder que l'équipe organisatrice.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de profils.</returns>
    /// <response code="200">Page de profils.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PageResponse<MemberResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResponse<MemberResponse>>> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool team = false,
        CancellationToken cancellationToken = default)
    {
        var requete = unitOfWork.ReadRepository<ApplicationUser>().Query
            .Where(user => user.IsActive && user.Slug != null);

        if (team)
        {
            requete = requete.Where(user => user.IsTeam);
        }

        requete = requete.OrderBy(user => user.LastName).ThenBy(user => user.FirstName);

        return await PageOfAsync(
            requete,
            new PageRequest(page, pageSize),
            ApiProjections.ToResponse,
            cancellationToken);
    }

    /// <summary>Lit le profil du membre connecté.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le profil du membre connecté.</returns>
    /// <response code="200">Profil trouvé.</response>
    /// <response code="404">Compte introuvable ou désactivé.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<MemberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberResponse>> LireSonProfil(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } id)
        {
            throw new DomainException("forbidden", "Connectez-vous pour lire votre profil.");
        }

        // Un compte authentifié peut avoir été désactivé entre l'émission du cookie et
        // l'appel : l'absence est une absence, pas une panne.
        var membre = await unitOfWork.ReadRepository<ApplicationUser>().Query
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        return membre is null ? NotFound() : Ok(ApiProjections.ToResponse(membre));
    }

    /// <summary>Lit un profil public par son slug.</summary>
    /// <param name="slug">Slug du membre.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le profil demandé.</returns>
    /// <response code="200">Profil trouvé.</response>
    /// <response code="404">Profil introuvable ou inactif.</response>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType<MemberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberResponse>> Lire(string slug, CancellationToken cancellationToken)
    {
        var membre = await unitOfWork.ReadRepository<ApplicationUser>().Query
            .FirstOrDefaultAsync(user => user.Slug == slug && user.IsActive, cancellationToken);

        return membre is null ? NotFound() : Ok(ApiProjections.ToResponse(membre));
    }

    /// <summary>Met à jour le profil du membre connecté.</summary>
    /// <param name="request">Nouveau profil.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le profil mis à jour.</returns>
    /// <remarks>
    /// Le membre ne modifie que les champs qu'il possède. Son adresse électronique, son mot
    /// de passe, son rôle et son appartenance à l'équipe relèvent du parcours de compte ou
    /// de l'administration, et passent donc par d'autres points de terminaison.
    /// </remarks>
    /// <response code="200">Profil mis à jour.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="404">Compte introuvable ou désactivé.</response>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType<MemberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberResponse>> MettreAJourSonProfil(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var id = RequireUser(currentUser);

        var membre = await unitOfWork.ReadRepository<ApplicationUser>().Query
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (membre is null)
        {
            return NotFound();
        }

        // Champs nommés, et `profession` renvoyée telle quelle : la fonction est saisie par
        // l'administration, au moment où elle inscrit le membre dans l'équipe. `UpdateProfile`
        // affecte toujours le champ qu'on lui donne, et son dixième paramètre vaut `null` par
        // défaut — l'omettre faisait donc disparaître le poste de la fiche à chaque
        // modification de profil, sans qu'aucune erreur ne le signale.
        membre.UpdateProfile(
            request.FirstName,
            request.LastName,
            bio: request.Bio,
            city: request.City,
            photoUrl: request.PhotoUrl,
            gitHubUsername: request.GitHubUsername,
            linkedInUrl: request.LinkedInUrl,
            xUrl: request.XUrl,
            technologies: request.Technologies,
            profession: membre.Profession);

        unitOfWork.Repository<ApplicationUser>().Update(membre);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToResponse(membre));
    }

    /// <summary>Supprime définitivement le compte du membre connecté.</summary>
    /// <param name="request">Double confirmation de la suppression.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <remarks>
    /// L'opération est irréversible : le compte disparaît, ainsi que les articles,
    /// ressources, projets, événements et commentaires dont il est l'auteur. Les
    /// commentaires qu'il a reçus, les signalements qu'il a déposés et ses inscriptions à
    /// des événements disparaissent également.
    /// <para>
    /// Deux conséquences sont voulues et méritent d'être connues avant de confirmer :
    /// supprimer un commentaire emporte les réponses qui lui sont rattachées, et supprimer
    /// un événement emporte les inscriptions des autres membres. Le fil de discussion et
    /// la liste des participants ne peuvent pas être conservés sans leur racine, et la
    /// conservation aurait exigé de réattribuer ces contenus à quelqu'un d'autre — ce que
    /// l'interface de suppression refuse de faire implicitement.
    /// </para>
    /// <para>
    /// Le dernier administrateur actif ne peut pas se supprimer : il n'existe alors plus
    /// personne pour administrer le site depuis son interface.
    /// </para>
    /// </remarks>
    /// <response code="204">Compte supprimé.</response>
    /// <response code="400">Confirmation absente ou incomplète.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="404">Compte introuvable ou désactivé.</response>
    /// <response code="409">Le compte est le dernier administrateur actif.</response>
    [HttpDelete("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SupprimerSonCompte(
        [FromBody] DeleteOwnAccountRequest request,
        CancellationToken cancellationToken)
    {
        var id = RequireUser(currentUser);

        var membre = await unitOfWork.ReadRepository<ApplicationUser>().Query
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (membre is null)
        {
            return NotFound();
        }

        // Les deux confirmations sont vérifiées avant tout effet : une demande partielle
        // ne doit rien supprimer, même si elle est valide par ailleurs.
        if (!request.ConfirmPermanentDeletion)
        {
            throw new DomainException(
                "confirmation_required",
                "Confirmez que vous comprenez que cette suppression est définitive.");
        }

        if (!string.Equals(
                request.ConfirmEmail?.Trim(),
                membre.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(
                "confirmation_mismatch",
                "L'adresse de confirmation ne correspond pas à celle de votre compte.");
        }

        if (await EstDernierAdministrateurAsync(membre, cancellationToken))
        {
            throw new DomainException(
                "last_admin",
                "Impossible de supprimer le dernier compte d'administrateur actif.");
        }

        await accountDeletion.DeleteAsync(id, cancellationToken);

        // La session survit à la suppression du compte dans le navigateur : sans cette
        // déconnexion, l'appelant resterait authentifié par un cookie sans compte associé.
        await signInManager.SignOutAsync();

        return NoContent();
    }

    /// <summary>
    /// Indique si un compte est le dernier administrateur actif du site.
    /// </summary>
    /// <param name="compte">Compte concerné, déjà chargé.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns><see langword="true"/> si aucun autre administrateur actif ne subsiste.</returns>
    /// <remarks>
    /// Même règle que dans l'administration des comptes, appliquée ici à la suppression
    /// volontaire : se retirer soi-même est permis, laisser le site sans administrateur ne
    /// l'est pas.
    /// </remarks>
    private async Task<bool> EstDernierAdministrateurAsync(
        ApplicationUser compte,
        CancellationToken cancellationToken)
    {
        if (!await userManager.IsInRoleAsync(compte, RoleNames.Admin))
        {
            return false;
        }

        var maintenant = DateTimeOffset.UtcNow;

        var admins = await userManager.GetUsersInRoleAsync(RoleNames.Admin);
        cancellationToken.ThrowIfCancellationRequested();

        return !admins.Any(
            admin => admin.Id != compte.Id
                && admin.IsActive
                && (admin.LockoutEnd is null || admin.LockoutEnd <= maintenant));
    }
}
