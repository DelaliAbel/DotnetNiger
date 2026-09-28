using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Abonnement public à la lettre d'information.
/// </summary>
[Route("api/newsletter")]
public sealed partial class NewsletterController(
    IUnitOfWork unitOfWork,
    IEmailDispatcher emails,
    IOptions<EmailOptions> emailOptions,
    TimeProvider clock,
    ILogger<NewsletterController> logger) : ApiControllerBase
{
    /// <summary>
    /// Demande l'abonnement d'une adresse à la lettre d'information.
    /// </summary>
    /// <param name="request">Adresse à abonner.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide ; un courriel de confirmation a été mis en file.</returns>
    /// <remarks>
    /// La réponse ne dit pas si l'adresse était déjà abonnée, et ne diffère pas entre une
    /// première demande et un réabonnement : ce rythme est exactement celui d'un formulaire
    /// qui se pilote pour savoir qui lit votre site. Seule la boîte de réception permet de
    /// le savoir.
    /// </remarks>
    /// <response code="202">Demande enregistrée.</response>
    /// <response code="400">Adresse absente ou invalide.</response>
    /// <response code="429">Trop de demandes depuis ce réseau.</response>
    [HttpPost]
    [AllowAnonymous]
    [Route("subscriptions")]
    [EnableRateLimiting("formulaire")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Sabonner(
        [FromBody] SubscribeToNewsletterRequest request,
        CancellationToken cancellationToken)
    {
        var maintenant = clock.GetUtcNow().UtcDateTime;
        var (jeton, empreinte) = SubscriptionToken.Creer();
        var cleDesabonnement = SubscriptionToken.CreerCleDesabonnement();

        var abonnement = await unitOfWork.ReadRepository<NewsletterSubscription>().Query
            .FirstOrDefaultAsync(candidat => candidat.Email == request.Email, cancellationToken);

        if (abonnement is null)
        {
            abonnement = NewsletterSubscription.Create(request.Email, empreinte, cleDesabonnement, maintenant);
            unitOfWork.Repository<NewsletterSubscription>().Add(abonnement);
        }
        else
        {
            // Déjà abonnée : on ne renvoie pas un second lien d'abonnement, ce serait une façon
            // de tester qui l'est. Mais le jeton est renouvelé, pour qu'un lien de
            // confirmation perdu reste rattrapable.
            if (abonnement.IsConfirmed)
            {
                DemandeIgnoree(logger);
                return Accepted();
            }

            abonnement.RequestAgain(empreinte, maintenant);
            unitOfWork.Repository<NewsletterSubscription>().Update(abonnement);
        }

        await emails.DispatchAsync(
            EmailTemplateKeys.NewsletterConfirmation,
            request.Email,
            EmailValues.De(
                (EmailVariables.UrlDesabonnement, BuildConfirmationUrl(jeton))),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        DemandeEnregistree(logger);

        return Accepted();
    }

    /// <summary>
    /// Confirme l'abonnement à la suite du courriel reçu.
    /// </summary>
    /// <param name="token">Jeton figurant dans le lien reçu.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide.</returns>
    /// <remarks>
    /// Un jeton absent, falsifié, déjà utilisé ou expiré ne distingue pas l'erreur : la
    /// réponse est identique, pour ne pas permettre de vérifier un jeton au hasard.
    /// </remarks>
    /// <response code="202">Confirmation traitée.</response>
    [HttpGet]
    [AllowAnonymous]
    [Route("subscriptions/confirm")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Confirmer(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var maintenant = clock.GetUtcNow().UtcDateTime;
        var empreinte = SubscriptionToken.Hasher(token);

        var abonnement = await unitOfWork.ReadRepository<NewsletterSubscription>().Query
            .FirstOrDefaultAsync(candidat => candidat.TokenHash == empreinte, cancellationToken);

        if (abonnement is null)
        {
            JetonRejete(logger);
            return Accepted();
        }

        if (!abonnement.Confirm(empreinte, maintenant))
        {
            JetonRejete(logger);
            return Accepted();
        }

        unitOfWork.Repository<NewsletterSubscription>().Update(abonnement);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        AbonnementConfirme(logger);

        return Accepted();
    }

    /// <summary>
    /// Rompt l'abonnement.
    /// </summary>
    /// <param name="key">Clé figurant dans le lien de désabonnement.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Réponse vide ; un courriel de confirmation de désabonnement a été mis en file.</returns>
    /// <remarks>
    /// Le désabonnement ne demande aucune confirmation supplémentaire : exiger un second clic
    /// laisserait les personnes qui ne seguiu plus la lettre coincées sur une liste qu'elles
    /// veulent quitter. Le courriel de confirmation leur permet surtout de vérifier qu'on n'a
    /// pas rompu un abonnement par erreur.
    /// </remarks>
    /// <response code="202">Désabonnement traité.</response>
    [HttpGet]
    [AllowAnonymous]
    [Route("unsubscribe")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Desabonner(
        [FromQuery] string key,
        CancellationToken cancellationToken)
    {
        var maintenant = clock.GetUtcNow().UtcDateTime;

        var abonnement = await unitOfWork.ReadRepository<NewsletterSubscription>().Query
            .FirstOrDefaultAsync(candidat => candidat.UnsubscribeKey == key, cancellationToken);

        if (abonnement is null)
        {
            JetonRejete(logger);
            return Accepted();
        }

        if (!abonnement.Unsubscribe(key, maintenant))
        {
            JetonRejete(logger);
            return Accepted();
        }

        unitOfWork.Repository<NewsletterSubscription>().Update(abonnement);

        // Confirmation au demandeur : un désabonnement demandé par erreur depuis une adresse
        // partagée est réparable seulement si la personne voit qu'il a bien été pris en compte.
        await emails.DispatchAsync(
            EmailTemplateKeys.NewsletterUnsubscribed,
            abonnement.Email,
            EmailValues.De(
                (EmailVariables.UrlCompte, BuildAccountUrl())),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        DesabonnementTraite(logger);

        return Accepted();
    }

    private string BuildConfirmationUrl(string jeton) =>
        $"{emailOptions.Value.BuildUrl("/api/newsletter/subscriptions/confirm")}?token={Uri.EscapeDataString(jeton)}";

    private string BuildAccountUrl() => emailOptions.Value.BuildUrl("/compte");

    [LoggerMessage(
        EventId = 120,
        Level = LogLevel.Information,
        Message = "Demande d'abonnement a la lettre enregistree")]
    private static partial void DemandeEnregistree(ILogger logger);

    [LoggerMessage(
        EventId = 121,
        Level = LogLevel.Debug,
        Message = "Demande d'abonnement ignoree pour une adresse deja confirmee")]
    private static partial void DemandeIgnoree(ILogger logger);

    [LoggerMessage(
        EventId = 122,
        Level = LogLevel.Information,
        Message = "Abonnement a la lettre confirme")]
    private static partial void AbonnementConfirme(ILogger logger);

    [LoggerMessage(
        EventId = 123,
        Level = LogLevel.Information,
        Message = "Desabonnement a la lettre traite")]
    private static partial void DesabonnementTraite(ILogger logger);

    [LoggerMessage(
        EventId = 125,
        Level = LogLevel.Warning,
        Message = "Jeton d'abonnement rejete")]
    private static partial void JetonRejete(ILogger logger);
}

