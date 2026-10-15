using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Services.Inscriptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Formulaire d'inscription du formulaire de la fiche événement.
/// </summary>
/// <param name="Slug">Slug de l'événement, seule information transmise par le navigateur.</param>
/// <param name="Retour">
/// Adresse de retour, pour que l'annulation faite depuis l'espace membre ramène à
/// « Mes rencontres » plutôt qu'à la fiche de l'événement.
/// </param>
public sealed record InscriptionEvenementInput(string? Slug, string? Retour = null);

/// <summary>
/// Inscription et désinscription depuis la fiche d'un événement.
/// </summary>
/// <remarks>
/// Le formulaire est rendu en HTML simple et soumis à ce contrôleur, comme le font
/// déjà la connexion, l'inscription et la lettre d'information. Un Blazor interactif
/// serait plus élégant, mais il imposerait un aller-retour du serveur pour simple chose
/// qu'un envoi de formulaire rend déjà parfaitement : le membre clique, la place est
/// prise, la page revient avec la confirmation.
/// </remarks>
[Route("evenements")]
public sealed class InscriptionsEvenementController : FormulaireControllerBase
{
    private readonly IUnitOfWork _uniteDeTravail;
    private readonly ICurrentUserService _utilisateur;
    private readonly ServiceInscriptionEvenement _service;

    /// <summary>Construit le contrôleur.</summary>
    /// <param name="uniteDeTravail">Unité de travail.</param>
    /// <param name="utilisateur">Service d'identité de la requête.</param>
    /// <param name="service">Service des inscriptions aux événements.</param>
    public InscriptionsEvenementController(
        IUnitOfWork uniteDeTravail,
        ICurrentUserService utilisateur,
        ServiceInscriptionEvenement service)
    {
        _uniteDeTravail = uniteDeTravail;
        _utilisateur = utilisateur;
        _service = service;
    }

    /// <summary>Prend la place du membre sur un événement.</summary>
    /// <param name="input">Slug de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Redirection vers la fiche, confirmation ou erreur.</returns>
    /// <remarks>
    /// Le jeton antiforgery est exigé : sans lui, n'importe quelle page tierce pourrait
    /// inscrire silencieusement un visiteur en son nom, en exploitant sa session.
    /// </remarks>
    [HttpPost("inscription")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscrire(
        [FromForm] InscriptionEvenementInput input,
        CancellationToken cancellationToken)
    {
        var slug = input.Slug?.Trim() ?? string.Empty;
        var retour = $"/evenements/{slug}";

        if (!EstMembre(HttpContext, _utilisateur))
        {
            return RedirectionConnexion(retour);
        }

        var evenement = await TrouverAsync(slug, cancellationToken);

        if (evenement is null)
        {
            return RedirectionAvecMessage("/evenements", "erreur", "Cet événement n'existe pas.");
        }

        try
        {
            var resultat = await _service.InscrireAsync(
                evenement.Id,
                _utilisateur.UserId!.Value,
                cancellationToken);

            return resultat.Reussi
                ? RedirectionAvecMessage(retour, "confirmation", "Votre place est confirmée. Rendez-vous à l'événement !")
                : RedirectionAvecMessage(retour, "erreur", resultat.Message);
        }
        catch (DomainException erreur)
        {
            return RedirectionAvecMessage(retour, "erreur", erreur.Message);
        }
    }

    /// <summary>Libère la place du membre sur un événement.</summary>
    /// <param name="input">Slug de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Redirection vers la fiche, confirmation ou erreur.</returns>
    [HttpPost("desinscription")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desinscrire(
        [FromForm] InscriptionEvenementInput input,
        CancellationToken cancellationToken)
    {
        var slug = input.Slug?.Trim() ?? string.Empty;

        // L'annulation peut partir de deux endroits : la fiche de l'événement, où l'on
        // reste sur place, et l'espace membre, où l'on revient à « Mes rencontres ». La
        // destination demandée est restreinte à une adresse interne du site, sinon le
        // formulaire deviendrait un relais de redirection vers n'importe quelle adresse.
        var retourInterne = EstAdresseInterne(input.Retour) ? input.Retour!.Trim() : $"/evenements/{slug}";

        if (!EstMembre(HttpContext, _utilisateur))
        {
            return RedirectionConnexion(retourInterne);
        }

        var evenement = await TrouverAsync(slug, cancellationToken);

        if (evenement is null)
        {
            return RedirectionAvecMessage("/evenements", "erreur", "Cet événement n'existe pas.");
        }

        var resultat = await _service.AnnulerAsync(
            evenement.Id,
            _utilisateur.UserId!.Value,
            cancellationToken);

        return RedirectionAvecMessage(
            retourInterne,
            resultat.Reussi ? "confirmation" : "erreur",
            resultat.Reussi ? "Votre inscription est annulée." : resultat.Message);
    }

    /// <summary>
    /// Indique si une adresse reste sur ce site.
    /// </summary>
    /// <remarks>
    /// Une adresse absolue vers un autre domaine serait une sortie du site ; un chemin
    /// interne, lui, est sûr. Les deux points d'entrée possibles — la fiche et l'espace
    /// membre — ne sont ainsi pas confondus.
    /// </remarks>
    private static bool EstAdresseInterne(string? adresse)
    {
        if (string.IsNullOrWhiteSpace(adresse))
        {
            return false;
        }

        var valeur = adresse.Trim();

        return valeur.StartsWith('/')
            && !valeur.StartsWith("//", StringComparison.Ordinal)
            && !valeur.StartsWith("/\\", StringComparison.Ordinal);
    }

    /// <summary>Retrouve l'événement désigné par le formulaire.</summary>
    /// <param name="slug">Slug de l'événement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'événement, ou <see langword="null"/> s'il n'existe pas.</returns>
    private Task<CommunityEvent?> TrouverAsync(string slug, CancellationToken cancellationToken)
        => _uniteDeTravail.ReadRepository<CommunityEvent>().Query
            .FirstOrDefaultAsync(candidat => candidat.Slug == slug, cancellationToken);
}