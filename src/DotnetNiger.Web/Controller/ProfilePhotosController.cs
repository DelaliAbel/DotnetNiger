using DotnetNiger.Application.Abstractions;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Photos de profil déposées par les membres.
/// </summary>
/// <remarks>
/// Les photos vivent en base et sont servies ici plutôt que depuis <c>wwwroot</c>. Un
/// fichier lisible directement par son URL serait la seule photo du site sans contrôle d'accès : balise <c>img</c> pourrait la charger depuis n'importe quelle page, et son
/// adresse serait diffuseable sans le dire.
/// </remarks>
[Route("api/photos-profil")]
[Tags("Photos de profil")]
[Authorize]
public sealed class ProfilePhotosController(
    DepotPhotosProfil depot,
    ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>Dépose une photo et la remplace si le membre en avait déjà une.</summary>
    /// <param name="file">Fichier image, JPEG, PNG ou WebP, 2 Mo au plus.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'adresse de la photo déposée.</returns>
    /// <response code="201">Photo déposée.</response>
    /// <response code="400">Fichier absent, trop lourd ou de format non accepté.</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PhotoDeposeeResponse>> Deposer(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } membreId)
        {
            return Unauthorized();
        }

        if (file is null || file.Length is 0)
        {
            return Problem(
                "Aucun fichier n'a été reçu.",
                "photo_absente");
        }

        // La taille est bornée avant la lecture : sans cela, un fichier de plusieurs
        // gigaoctets serait chargé en mémoire avant qu'on ait pu le refuser.
        if (file.Length > Domain.Entities.Identity.ProfilePhoto.MaxBytes)
        {
            return Problem(
                "La photo dépasse 2 Mo.",
                "photo_trop_lourde");
        }

        var contenu = new byte[file.Length];
        await using (var flux = file.OpenReadStream())
        {
            await flux.ReadExactlyAsync(contenu, cancellationToken);
        }

        try
        {
            await depot.RemplacerAsync(
                membreId,
                contenu,
                file.ContentType,
                cancellationToken);
        }
        catch (DomainException erreur)
        {
            return Problem(erreur.Message, erreur.Code);
        }

        return Created(
            $"/api/photos-profil/moi",
            new PhotoDeposeeResponse("/api/photos-profil/moi"));
    }

    /// <summary>Retire la photo du membre connecté.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <response code="204">Photo retirée, ou aucune photo n'existait.</response>
    [HttpDelete("moi")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Supprimer(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is { } membreId)
        {
            await depot.SupprimerAsync(membreId, cancellationToken);
        }

        return NoContent();
    }

    /// <summary>Lit la photo du membre connecté.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les octets de la photo.</returns>
    /// <response code="200">Photo renvoyée.</response>
    /// <response code="404">Le membre n'a pas de photo.</response>
    [HttpGet("moi")]
    // Les trois types concrets, et non « image/* » : ASP.NET Core refuse au démarrage un
    // type média générique, quand il construit le modèle d'action. L'application ne
    // demographics pas si on l'omet, mais la déclaration du contrat serait fausse.
    [Produces("image/jpeg", "image/png", "image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LireMoi(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } membreId)
        {
            return Unauthorized();
        }

        var photo = await depot.LireAsync(membreId, cancellationToken);

        return photo is null ? NotFound() : File(photo.Contenu, photo.TypeContenu);
    }

    /// <summary>Lit la photo d'un membre donné.</summary>
    /// <remarks>
    /// Ouvert à tout membre connecté : une photo de profil est ce que les autres membres
    /// ont le droit de voir, la masquer rendrait le fil illisible. La lecture est toujours
    /// faite par identifiant, jamais par le nom du fichier.
    /// </remarks>
    /// <param name="membreId">Identifiant du membre.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les octets de la photo.</returns>
    /// <response code="200">Photo renvoyée.</response>
    /// <response code="404">Ce membre n'a pas de photo.</response>
    [HttpGet("{membreId:guid}")]
    // Les trois types concrets, et non « image/* » : ASP.NET Core refuse au démarrage un
    // type média générique, quand il construit le modèle d'action. L'application ne
    // demographics pas si on l'omet, mais la déclaration du contrat serait fausse.
    [Produces("image/jpeg", "image/png", "image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lire(Guid membreId, CancellationToken cancellationToken)
    {
        var photo = await depot.LireAsync(membreId, cancellationToken);

        return photo is null ? NotFound() : File(photo.Contenu, photo.TypeContenu);
    }
}

/// <summary>Adresse de la photo déposée.</summary>
/// <param name="Url">Adresse à utiliser dans une balise <c>img</c>.</param>
public sealed record PhotoDeposeeResponse(string Url);