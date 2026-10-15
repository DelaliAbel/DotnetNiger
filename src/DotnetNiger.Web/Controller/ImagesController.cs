using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Images de contenu déposées par les administrateurs : couvertures d'articles et
/// illustrations du corps d'une publication.
/// </summary>
/// <remarks>
/// Les images vivent en base et sont servies ici plutôt que depuis <c>wwwroot</c> — même
/// logique que les photos de profil. À la différence de ces dernières, une image de
/// contenu est publique dès sa mise en ligne : la couverture d'un article se voit par les
/// visiteurs, la lecture est donc ouverte. Le dépôt, lui, exige une session authentifiée :
/// seul le contenu éditorial est rédigé en administration.
/// <para>
/// Le fichier déposé n'est jamais renvoyé tel quel : le type MIME réel est reconnu à
/// partir des octets, puis retourné dans l'en-tête — c'est le contenu qui décide de ce que
/// le navigateur affichera.
/// </para>
/// </remarks>
[Route("api/images")]
[Tags("Images")]
public sealed class ImagesController(DepotImagesContenu depot) : ControllerBase
{
    /// <summary>Dépose une image de contenu.</summary>
    /// <param name="file">Fichier image, JPEG, PNG ou WebP, 5 Mo au plus.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'adresse de l'image déposée.</returns>
    /// <response code="201">Image déposée.</response>
    /// <response code="400">Fichier absent, trop lourd ou de format non accepté.</response>
    /// <response code="401">Visiteur non connecté.</response>
    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ImageDeposeeResponse>> Deposer(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length is 0)
        {
            return DepôtRefuse("image_absente", "Aucun fichier n'a été reçu.");
        }

        // La taille est bornée avant la lecture : sans cela, un fichier de plusieurs
        // gigaoctets serait chargé en mémoire avant qu'on ait pu le refuser.
        if (file.Length > ContentImage.MaxBytes)
        {
            return DepôtRefuse(
                "image_trop_grosse",
                $"L'image dépasse {ContentImage.MaxBytes / 1024 / 1024} Mo.");
        }

        var contenu = new byte[file.Length];
        await using (var flux = file.OpenReadStream())
        {
            await flux.ReadExactlyAsync(contenu, cancellationToken);
        }

        try
        {
            var image = await depot.DeposerAsync(contenu, file.ContentType, cancellationToken);

            return Created(
                $"/api/images/{image.Id}",
                new ImageDeposeeResponse(image.Id, $"/api/images/{image.Id}"));
        }
        catch (DomainException erreur)
        {
            return DepôtRefuse(erreur.Code, erreur.Message);
        }
    }

    /// <summary>Lit une image de contenu, pour l'afficher sur le site.</summary>
    /// <remarks>
    /// Ouvert à tous : une couverture d'article se voit par les visiteurs, une session en
    /// cacherait inutilement. L'image étant écrite une fois pour toutes à son dépôt, elle
    /// est mise en cache de façon immuable.
    /// </remarks>
    /// <param name="id">Identifiant de l'image.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les octets de l'image.</returns>
    /// <response code="200">Image renvoyée.</response>
    /// <response code="404">L'image n'existe pas.</response>
    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
    // Les trois types concrets, et non « image/* » : ASP.NET Core refuse au démarrage un
    // type média générique, quand il construit le modèle d'action.
    [Produces("image/jpeg", "image/png", "image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lire(Guid id, CancellationToken cancellationToken)
    {
        var image = await depot.LireAsync(id, cancellationToken);

        return image is null ? NotFound() : File(image.Contenu, image.TypeContenu);
    }

    /// <summary>
/// Construit une réponse de dépôt refusée, au format problem details.
/// </summary>
    /// <param name="code">Code d'erreur, porté par <c>instance</c>.</param>
    /// <param name="message">Raison du refus, pour l'administrateur.</param>
    /// <returns>Réponse 400 documentée.</returns>
    private BadRequestObjectResult DepôtRefuse(string code, string message) =>
        BadRequest(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Téléversement refusé.",
            Detail = message,
            Instance = code,
        });
}

/// <summary>Adresse de l'image déposée.</summary>
/// <param name="Id">Identifiant de l'image.</param>
/// <param name="Url">Adresse à utiliser dans une balise <c>img</c> ou dans le corps d'un article.</param>
public sealed record ImageDeposeeResponse(Guid Id, string Url);