using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
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
    ICurrentUserService currentUser) : ApiControllerBase
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

        membre.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Bio,
            request.City,
            request.PhotoUrl,
            request.GitHubUsername,
            request.LinkedInUrl,
            request.XUrl,
            request.Technologies);

        unitOfWork.Repository<ApplicationUser>().Update(membre);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToResponse(membre));
    }
}
