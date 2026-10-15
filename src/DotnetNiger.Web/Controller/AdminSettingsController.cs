using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using DotnetNiger.Web.Services.Reglages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Réglages du site : logo, adresse de contact et canaux de la communauté.
/// </summary>
/// <remarks>
/// Ces valeurs sont dans la base et non dans un fichier : ce sont celles qu'une
/// administratrice doit pouvoir changer sans passer par le serveur. Ce que le site ne doit
/// pas laisser modifier depuis l'interface — la base, le relais de courriel, les clés de
/// connexion externe — reste dans la configuration de déploiement.
/// </remarks>
[Route("api/admin/settings")]
[Tags("Réglages du site")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminSettingsController(
    ICurrentUserService currentUser,
    ServiceReglages reglages) : ApiControllerBase
{
    /// <summary>Lit les réglages du site.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les réglages en vigueur.</returns>
    /// <response code="200">Réglages lus.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpGet]
    [ProducesResponseType<SiteSettingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SiteSettingsResponse>> Lire(CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var valeurs = await reglages.LireAsync(cancellationToken);

        return Ok(ToResponse(valeurs));
    }

    /// <summary>Remplace les réglages du site.</summary>
    /// <param name="request">Nouvelles valeurs. Un champ vide retire le réglage.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les réglages après enregistrement.</returns>
    /// <response code="200">Réglages enregistrés.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpPut]
    [ProducesResponseType<SiteSettingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SiteSettingsResponse>> MettreAJour(
        [FromBody] UpdateSiteSettingsRequest request,
        CancellationToken cancellationToken)
    {
        RequireAdministrateur(currentUser);

        var modifications = new (string Cle, string? Valeur)[]
        {
            (ClesReglage.Logo, request.Logo),
            (ClesReglage.Courriel, request.Email),
            (ClesReglage.Whatsapp, request.WhatsappUrl),
            (ClesReglage.Github, request.GithubUrl),
            (ClesReglage.Linkedin, request.LinkedinUrl),
            (ClesReglage.X, request.XUrl),
            (ClesReglage.Youtube, request.YoutubeUrl),
            (ClesReglage.Facebook, request.FacebookUrl),
        };

        // Les réglages sont écrits l'un après l'autre : une clé refusée arrête l'écriture
        // et dit laquelle. Une demi-sauvegarde laisserait l'écran et le site en désaccord.
        foreach (var (cle, valeur) in modifications)
        {
            var resultat = await reglages.DefinirAsync(cle, valeur, cancellationToken);

            if (!resultat.Reussi)
            {
                throw new DomainException("setting_rejected", resultat.Message);
            }
        }

        return Ok(ToResponse(await reglages.LireAsync(cancellationToken)));
    }

    private static SiteSettingsResponse ToResponse(ReglagesVue valeurs) => new(
        valeurs.Logo,
        valeurs.Courriel,
        valeurs.Whatsapp,
        valeurs.Github,
        valeurs.Linkedin,
        valeurs.X,
        valeurs.Youtube,
        valeurs.Facebook);
}