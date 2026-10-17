using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Infrastructure.Content;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Photos de l'équipe organisatrice, lisibles par tout visiteur.
/// </summary>
/// <remarks>
/// La page « À propos » est publique : une carte qui montre une photo à un membre connecté
/// et un cercle vide à un visiteur serait fausse pour la moitié de ses lecteurs.
/// <para>
/// L'ouverture au public est donc portée par ce contrôleur dédié, et non par une exception
/// sur le contrôleur des photos de profil. Ce dernier reste réservé aux membres : une photo
/// de profil n'a pas à être dans la rue, et l'exception serait le genre de ligne qu'on
/// laisse en place après l'avoir écrite pour un seul écran.
/// </para>
/// <para>
/// Seul compte un compte actif et marqué « équipe ». Toute autre adresse répond « introuvable »,
/// ce qui évite de confirmer l'existence d'un compte.
/// </para>
/// </remarks>
[Route("api/photos-equipe")]
[Tags("Photos de l'équipe")]
[AllowAnonymous]
public sealed class TeamPhotosController(
    DepotPhotosProfil depot,
    IUnitOfWork comptes) : ControllerBase
{
    /// <summary>
    /// Adresse de la photo d'un membre de l'équipe, à mettre dans une balise <c>img</c>.
    /// </summary>
    /// <param name="membreId">Identifiant du membre.</param>
    /// <returns>L'adresse de la photo.</returns>
    /// <remarks>
    /// Une seule écriture de la route : la recomposer dans chaque écran produirait des
    /// liens qui divergent d'un slash près, et une image cassée sur une seule page — sans
    /// indice de ce qui cloche.
    /// </remarks>
    public static string AdresseDe(Guid membreId) => $"/api/photos-equipe/{membreId}";

    /// <summary>Photo d'un membre de l'équipe.</summary>
    /// <param name="membreId">Identifiant du membre.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les octets de la photo.</returns>
    /// <response code="200">Photo renvoyée.</response>
    /// <response code="404">Ce membre n'est pas de l'équipe, ou n'a pas de photo.</response>
    [HttpGet("{membreId:guid}")]
    // Les trois types concrets, et non « image/* » : ASP.NET Core refuse au démarrage un
    // type média générique, quand il construit le modèle d'action.
    [Produces("image/jpeg", "image/png", "image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lire(Guid membreId, CancellationToken cancellationToken)
    {
        var estDeLEquipe = await comptes.ReadRepository<ApplicationUser>().Query
            .AsNoTracking()
            .AnyAsync(
                compte => compte.Id == membreId && compte.IsActive && compte.IsTeam,
                cancellationToken);

        if (!estDeLEquipe)
        {
            return NotFound();
        }

        var photo = await depot.LireAsync(membreId, cancellationToken);

        return photo is null ? NotFound() : File(photo.Contenu, photo.TypeContenu);
    }
}
