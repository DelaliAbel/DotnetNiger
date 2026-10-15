using DotnetNiger.Application.Abstractions;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Domain.Entities.Identity;
using DotnetNiger.Infrastructure.Content;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Services.Membres;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller;

/// <summary>Formulaire de mise à jour du profil, depuis l'espace membre.</summary>
public sealed class ProfilInput
{
    /// <summary>Prénom.</summary>
    public string? Prenom { get; set; }

    /// <summary>Nom de famille.</summary>
    public string? Nom { get; set; }

    /// <summary>Présentation.</summary>
    public string? Bio { get; set; }

    /// <summary>Ville.</summary>
    public string? Ville { get; set; }

    /// <summary>Ancienne adresse de photo, facultative et non exposée par le formulaire.</summary>
    public string? PhotoUrl { get; set; }

    /// <summary>Ancien champ GitHub, conservé pour les profils déjà renseignés.</summary>
    public string? GitHubUsername { get; set; }

    /// <summary>Ancien champ LinkedIn, conservé pour les profils déjà renseignés.</summary>
    public string? LinkedInUrl { get; set; }

    /// <summary>Ancien champ X, conservé pour les profils déjà renseignés.</summary>
    public string? XUrl { get; set; }

    /// <summary>Compétences déclarées.</summary>
    public string? Technologies { get; set; }

    /// <summary>Collection de liens libres : portfolio, GitHub, réseau social, etc.</summary>
    public List<LienMembreInput> Liens { get; set; } = [];
}

/// <summary>Formulaire de retrait définitif du compte.</summary>
/// <param name="Email">Adresse du compte, recopiée pour confirmer.</param>
/// <param name="Confirmation">Case à cocher confirmant la suppression définitive.</param>
public sealed record SuppressionCompteInput(string? Email, bool Confirmation);

/// <summary>
/// Mise à jour du profil et retrait du compte.
/// </summary>
/// <remarks>
/// Le retrait demande deux confirmations : recopier l'adresse puis cocher une case.
/// Une seule des deux laisserait à un clic perdu la destruction de tout ce que le membre
/// a publié — cette perte est définitive, la première règle du site.
/// </remarks>
[Route("compte")]
public sealed class ProfilController : FormulaireControllerBase
{
    private const string PageProfil = "/compte/profil";

    private readonly ICurrentUserService _utilisateur;
    private readonly ServiceMembres _service;
    private readonly IAccountDeletionService _accountDeletion;
    private readonly DepotPhotosProfil _photos;

    /// <summary>Construit le contrôleur.</summary>
    /// <param name="utilisateur">Service d'identité de la requête.</param>
    /// <param name="service">Service des membres.</param>
    /// <param name="accountDeletion">Service de retrait de compte.</param>
    /// <param name="photos">Dépôt des photos de profil.</param>
    public ProfilController(
        ICurrentUserService utilisateur,
        ServiceMembres service,
        IAccountDeletionService accountDeletion,
        DepotPhotosProfil photos)
    {
        _utilisateur = utilisateur;
        _service = service;
        _accountDeletion = accountDeletion;
        _photos = photos;
    }

    /// <summary>Enregistre les modifications du profil.</summary>
    /// <param name="input">Contenu du formulaire.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Redirection vers la page profil, confirmation ou erreur.</returns>
    [HttpPost("profil/enregistrer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enregistrer(
        [FromForm] ProfilInput input,
        CancellationToken cancellationToken)
    {
        if (!EstMembre(_utilisateur))
        {
            return RedirectionConnexion(PageProfil);
        }

        try
        {
            var enregistre = await _service.MettreAJourAsync(
                _utilisateur.UserId!.Value,
                input.Prenom ?? string.Empty,
                input.Nom ?? string.Empty,
                input.Bio,
                input.Ville,
                input.PhotoUrl,
                input.GitHubUsername,
                input.LinkedInUrl,
                input.XUrl,
                input.Technologies,
                cancellationToken);

            if (!enregistre)
            {
                return RedirectionAvecMessage(PageProfil, "erreur", "Le compte est introuvable.");
            }

            await _service.RemplacerLiensAsync(
                _utilisateur.UserId!.Value,
                input.Liens,
                cancellationToken);

            return RedirectionAvecMessage(PageProfil, "confirmation", "Votre profil est à jour.");
        }
        catch (DomainException erreur)
        {
            // Le domaine refuse une valeur fautive en donnant la raison : la
            // redirection la rend au membre plutôt que de l'avaler.
            return RedirectionAvecMessage(PageProfil, "erreur", erreur.Message);
        }
    }

    /// <summary>Dépose la photo de profil envoyée depuis la page du profil.</summary>
    /// <param name="file">Fichier image, JPEG, PNG ou WebP, 2 Mo au plus.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Redirection vers la page profil, confirmation ou erreur.</returns>
    [HttpPost("photo")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> DeposerPhoto(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (!EstMembre(_utilisateur))
        {
            return RedirectionConnexion(PageProfil);
        }

        if (file is null || file.Length is 0)
        {
            return RedirectionAvecMessage(PageProfil, "erreur", "Aucun fichier n'a été reçu.");
        }

        if (file.Length > ProfilePhoto.MaxBytes)
        {
            return RedirectionAvecMessage(PageProfil, "erreur", "La photo dépasse 2 Mo.");
        }

        var contenu = new byte[file.Length];
        await using (var flux = file.OpenReadStream())
        {
            await flux.ReadExactlyAsync(contenu, cancellationToken);
        }

        try
        {
            await _photos.RemplacerAsync(
                _utilisateur.UserId!.Value,
                contenu,
                file.ContentType,
                cancellationToken);

            return RedirectionAvecMessage(PageProfil, "confirmation", "Votre photo est à jour.");
        }
        catch (DomainException erreur)
        {
            return RedirectionAvecMessage(PageProfil, "erreur", erreur.Message);
        }
    }

    /// <summary>Retire la photo de profil du membre connecté.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Redirection vers la page profil.</returns>
    [HttpPost("photo/retirer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetirerPhoto(CancellationToken cancellationToken)
    {
        if (!EstMembre(_utilisateur))
        {
            return RedirectionConnexion(PageProfil);
        }

        var retiree = await _photos.SupprimerAsync(_utilisateur.UserId!.Value, cancellationToken);

        return RedirectionAvecMessage(
            PageProfil,
            "confirmation",
            retiree ? "Votre photo a été retirée." : "Vous n'aviez pas de photo.");
    }

    /// <summary>Retire définitivement le compte et ce qu'il a publié.</summary>
    /// <param name="input">Adresse recopiée et case de confirmation.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Redirection vers la connexion une fois le compte retiré.</returns>
    [HttpPost("suppression")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(
        [FromForm] SuppressionCompteInput input,
        CancellationToken cancellationToken)
    {
        if (!EstMembre(_utilisateur))
        {
            return RedirectionConnexion(PageProfil);
        }

        var adresse = (input.Email ?? string.Empty).Trim();
        var id = _utilisateur.UserId!.Value;

        // Les deux confirmations sont exigées avant tout effet : n'en omettre une seule
        // laisserait un clic suffit à effacer définitivement les contenus du membre.
        if (!string.Equals(adresse, User.Identity?.Name, StringComparison.OrdinalIgnoreCase)
            || !input.Confirmation)
        {
            return RedirectionAvecMessage(
                PageProfil,
                "erreur",
                "Il faut reprendre l'adresse du compte et cocher la case pour confirmer.");
        }

        try
        {
            await _accountDeletion.DeleteAsync(id, cancellationToken);
        }
        catch (DomainException erreur)
        {
            return RedirectionAvecMessage(PageProfil, "erreur", erreur.Message);
        }

        // Le compte a disparu : la session ne peut plus être valide. La redirection
        // vers la connexion ferme donc proprement la porte plutôt que de laisser une
        // page renvoyer vers un compte inexistant.
        return Redirect("/compte/connexion?desinscription=1");
    }
}
