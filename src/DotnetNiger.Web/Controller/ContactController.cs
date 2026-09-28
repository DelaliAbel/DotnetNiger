using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Exceptions;
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
/// Formulaire de contact public.
/// </summary>
[Route("api/contact")]
public sealed partial class ContactController(
    IUnitOfWork unitOfWork,
    IEmailDispatcher emails,
    IOptions<EmailOptions> emailOptions,
    TimeProvider clock,
    ILogger<ContactController> logger) : ApiControllerBase
{
    /// <summary>
    /// Dépose un message de contact.
    /// </summary>
    /// <param name="request">Message à déposer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le message déposé.</returns>
    /// <remarks>
    /// Le message est enregistré avant tout envoi, et les deux courriels partent par la file
    /// d'attente : le visiteur ne reste donc jamais en attente d'un serveur de courriel.
    /// </remarks>
    /// <response code="201">Message déposé.</response>
    /// <response code="400">Message incomplet ou trop long.</response>
    /// <response code="429">Trop de messages déposés depuis cette adresse réseau.</response>
    [HttpPost]
    [AllowAnonymous]
    [Route("messages")]
    [EnableRateLimiting("formulaire")]
    [ProducesResponseType<ContactMessageResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ContactMessageResponse>> Deposer(
        [FromBody] CreateContactMessageRequest request,
        CancellationToken cancellationToken)
    {
        var maintenant = clock.GetUtcNow().UtcDateTime;

        var message = ContactMessage.Create(
            request.Name,
            request.Email,
            request.Subject,
            request.Message,
            maintenant);

        unitOfWork.Repository<ContactMessage>().Add(message);

        // Accusé au demandeur : il sait ainsi que son message est arrivé, avant même qu'une
        // réponse n'existe. Sans cela, il en déduit souvent que l'envoi a échoué et réessaie.
        await emails.DispatchAsync(
            EmailTemplateKeys.ContactAcknowledged,
            message.Email,
            EmailValues.De(
                (EmailVariables.NomContact, message.Name),
                (EmailVariables.SujetContact, message.Subject),
                (EmailVariables.MessageContact, message.Body)),
            cancellationToken);

        // Copie à l'administration : c'est ce message qui déclenche la réponse, il ne peut pas
        // dépendre du filtre de la boîte du demandeur.
        var contact = emailOptions.Value.ContactAddress;

        if (!string.IsNullOrWhiteSpace(contact))
        {
            await emails.DispatchAsync(
                EmailTemplateKeys.ContactForwarded,
                contact,
                EmailValues.De(
                    (EmailVariables.NomContact, message.Name),
                    (EmailVariables.EmailContact, message.Email),
                    (EmailVariables.SujetContact, message.Subject),
                    (EmailVariables.MessageContact, message.Body)),
                cancellationToken);
        }
        else
        {
            AdresseAdminAbsente(logger);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        MessageDepose(logger, message.Subject);

        return Created($"/api/admin/contact/messages/{message.Id}", ToResponse(message));
    }

    private static ContactMessageResponse ToResponse(ContactMessage message) =>
        new(
            message.Id,
            message.Name,
            message.Email,
            message.Subject,
            message.Body,
            message.ReceivedAtUtc,
            message.HandledAtUtc);

    [LoggerMessage(
        EventId = 110,
        Level = LogLevel.Information,
        Message = "Message de contact reçu : {Sujet}")]
    private static partial void MessageDepose(ILogger logger, string sujet);

    [LoggerMessage(
        EventId = 111,
        Level = LogLevel.Warning,
        Message = "Message de contact recu sans adresse d'administration : la copie n'a pas ete envoyee")]
    private static partial void AdresseAdminAbsente(ILogger logger);
}

