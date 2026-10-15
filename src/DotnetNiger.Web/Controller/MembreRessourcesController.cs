using System.ComponentModel.DataAnnotations;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Content;
using DotnetNiger.Web.Controller.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller;

/// <summary>Formulaire HTML de dépôt d'une ressource par un membre.</summary>
[Route("compte/ressources")]
[Authorize]
public sealed class MembreRessourcesController(
    ICurrentUserService currentUser,
    DepotRessources depot) : FormulaireControllerBase
{
    /// <summary>
    /// Dépose une ressource depuis le fil de l'espace membre.
    /// </summary>
    /// <param name="input">Champs saisis dans le formulaire.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Retour vers le fil, avec confirmation ou erreur.</returns>
    [HttpPost("deposer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposer(
        [FromForm] DepotRessourceInput input,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } auteur)
        {
            return RedirectionConnexion("/compte/tableau-de-bord");
        }

        if (!ModelState.IsValid)
        {
            return Redirect("/compte/tableau-de-bord?erreur=depot-invalide");
        }

        try
        {
            await depot.DeposerAsync(
                auteur,
                input.Titre,
                input.Description,
                input.Categorie,
                input.LienExterne,
                input.Fournisseur,
                imageCouverture: null,
                contenu: null,
                cancellationToken);

            return Redirect("/compte/tableau-de-bord?confirmation=ressource-deposee");
        }
        catch (DomainException erreur)
        {
            return Redirect($"/compte/tableau-de-bord?erreur={Uri.EscapeDataString(erreur.Code)}");
        }
    }

    /// <summary>
    /// Corrige une ressource déposée par le membre connecté.
    /// </summary>
    /// <param name="id">Identifiant de la ressource à corriger.</param>
    /// <param name="input">Champs corrigés.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Retour vers le fil, avec confirmation ou erreur.</returns>
    [HttpPost("{id:guid}/corriger")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Corriger(
        Guid id,
        [FromForm] DepotRessourceInput input,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } auteur)
        {
            return RedirectionConnexion("/compte/tableau-de-bord");
        }

        if (!ModelState.IsValid)
        {
            return Redirect("/compte/tableau-de-bord?erreur=depot-invalide");
        }

        try
        {
            await depot.CorrigerAsync(
                auteur,
                id,
                input.Titre,
                input.Description,
                input.Categorie,
                input.LienExterne,
                input.Fournisseur,
                cancellationToken);

            return Redirect("/compte/tableau-de-bord?confirmation=ressource-corrigee");
        }
        catch (DomainException erreur)
        {
            return Redirect($"/compte/tableau-de-bord?erreur={Uri.EscapeDataString(erreur.Code)}");
        }
    }

    /// <summary>
    /// Retire définitivement une ressource déposée par le membre connecté.
    /// </summary>
    /// <remarks>
    /// La suppression est définitive : le formulaire qui la déclenche demande une
    /// confirmation explicite, et la méthode refuse toute ressource dont le membre n'est
    /// pas l'auteur.
    /// </remarks>
    /// <param name="id">Identifiant de la ressource à retirer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Retour vers le fil, avec confirmation ou erreur.</returns>
    [HttpPost("{id:guid}/retirer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Retirer(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } auteur)
        {
            return RedirectionConnexion("/compte/tableau-de-bord");
        }

        try
        {
            await depot.RetirerAsync(auteur, id, cancellationToken);

            return Redirect("/compte/tableau-de-bord?confirmation=ressource-retiree");
        }
        catch (DomainException erreur)
        {
            return Redirect($"/compte/tableau-de-bord?erreur={Uri.EscapeDataString(erreur.Code)}");
        }
    }
}

/// <summary>Valeurs reçues du formulaire de dépôt d'une ressource.</summary>
public sealed class DepotRessourceInput
{
    /// <summary>Titre affiché de la ressource.</summary>
    [Required, StringLength(200)]
    public string Titre { get; set; } = string.Empty;

    /// <summary>Description courte de la ressource.</summary>
    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Catégorie qui permet de la retrouver sur la page Ressources.</summary>
    [EnumDataType(typeof(ResourceCategory))]
    public ResourceCategory Categorie { get; set; }

    /// <summary>URL externe, facultative pour une ressource hébergée sur le site.</summary>
    [StringLength(2048)]
    public string? LienExterne { get; set; }

    /// <summary>Site ou éditeur de la ressource.</summary>
    [StringLength(120)]
    public string? Fournisseur { get; set; }
}
