using DotnetNiger.Application.Abstractions;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Services.Commentaires;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller;

/// <summary>Formulaire de dépôt d'un commentaire depuis une fiche.</summary>
/// <param name="ArticleId">Article commenté, si la fiche porte sur un article.</param>
/// <param name="EventId">Événement commenté, si la fiche porte sur un événement.</param>
/// <param name="Body">Texte du commentaire.</param>
/// <param name="Retour">Chemin de la fiche, vers lequel revenir après dépôt.</param>
public sealed record CommentaireInput(string? ArticleId, string? EventId, string? Body, string? Retour);

/// <summary>Formulaire de suppression d'un commentaire.</summary>
/// <param name="Id">Identifiant du commentaire à retirer.</param>
/// <param name="Retour">Chemin de la fiche, vers laquelle revenir.</param>
public sealed record SuppressionCommentaireInput(string? Id, string? Retour);

/// <summary>
/// Dépôt et suppression des commentaires depuis la fiche d'un article ou d'un événement.
/// </summary>
/// <remarks>
/// Même approche que l'inscription aux événements : un envoi de formulaire simple, rendu
/// par le pipeline antiforgery, plutôt qu'un composant interactif. Le commentaire
/// publié doit survivre au rechargement de la page ; un état côté client le ferait
/// disparaître, ce qui laisserait le membre dans le doute sur ce qui a été gardé.
/// </remarks>
[Route("commentaires")]
public sealed class CommentairesController : FormulaireControllerBase
{
    private readonly ICurrentUserService _utilisateur;
    private readonly ServiceCommentaires _service;

    /// <summary>Construit le contrôleur.</summary>
    /// <param name="utilisateur">Service d'identité de la requête.</param>
    /// <param name="service">Service des commentaires.</param>
    public CommentairesController(ICurrentUserService utilisateur, ServiceCommentaires service)
    {
        _utilisateur = utilisateur;
        _service = service;
    }

    /// <summary>Enregistre un commentaire depuis la fiche d'un article ou d'un événement.</summary>
    /// <param name="input">Contenu du formulaire.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Redirection vers la fiche, confirmation ou erreur.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposer(
        [FromForm] CommentaireInput input,
        CancellationToken cancellationToken)
    {
        var retour = CheminAutorise(input.Retour);

        if (!EstMembre(_utilisateur))
        {
            return RedirectionConnexion(retour);
        }

        var article = Guid.TryParse(input.ArticleId, out var idArticle) ? idArticle : (Guid?)null;
        var evenement = Guid.TryParse(input.EventId, out var idEvenement) ? idEvenement : (Guid?)null;

        var resultat = await _service.DeposerAsync(
            article,
            evenement,
            _utilisateur.UserId!.Value,
            input.Body ?? string.Empty,
            cancellationToken);

        return RedirectionAvecMessage(retour, resultat.Reussi ? "confirmation" : "erreur", resultat.Message);
    }

    /// <summary>Retire un commentaire depuis la fiche.</summary>
    /// <param name="input">Identifiant du commentaire et chemin de retour.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Redirection vers la fiche, confirmation ou erreur.</returns>
    [HttpPost("suppression")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(
        [FromForm] SuppressionCommentaireInput input,
        CancellationToken cancellationToken)
    {
        var retour = CheminAutorise(input.Retour);

        if (!EstMembre(_utilisateur))
        {
            return RedirectionConnexion(retour);
        }

        if (!Guid.TryParse(input.Id, out var commentaireId))
        {
            return RedirectionAvecMessage(retour, "erreur", "Commentaire introuvable.");
        }

        var resultat = await _service.SupprimerAsync(
            commentaireId,
            _utilisateur.UserId!.Value,
            cancellationToken);

        return RedirectionAvecMessage(retour, resultat.Reussi ? "confirmation" : "erreur", resultat.Message);
    }
}