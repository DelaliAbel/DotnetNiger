using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Notifications;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Infrastructure.Notifications;

/// <summary>
/// Expéédie un lot de courriels dus et consigne le résultat de chaque tentative.
/// </summary>
/// <remarks>
/// Séparé du <see cref="OutboxDispatcher"/> pour être vérifiable : la boucle de fond
/// n'appelle que <see cref="DrainAsync"/> en boucle, et un test peut l'appeler une fois
/// pour observer exactement ce qu'elle fait à un instant donné.
/// </remarks>
public sealed partial class OutboxProcessor
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _sender;
    private readonly TimeProvider _clock;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxProcessor> _logger;

    /// <summary>
    /// Initialise le processeur.
    /// </summary>
    /// <param name="unitOfWork">Unité de travail de la portée courante.</param>
    /// <param name="sender">Expéditeur des courriels.</param>
    /// <param name="clock">Source de temps.</param>
    /// <param name="options">Réglages du relais.</param>
    /// <param name="logger">Journalisation.</param>
    public OutboxProcessor(
        IUnitOfWork unitOfWork,
        IEmailSender sender,
        TimeProvider clock,
        IOptions<OutboxOptions> options,
        ILogger<OutboxProcessor> logger)
    {
        _unitOfWork = unitOfWork;
        _sender = sender;
        _clock = clock;

        // IOptions, et non le type nu : « Configure » n'enregistre que IOptions<T>. Demander
        // OutboxOptions directement empêche tout le conteneur de démarrer, et aucun test
        // d'intégration ne le voyait, faute de monter le même graphe que l'application.
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Expéédie tous les messages dont l'heure de tentative est atteinte.
    /// </summary>
    /// <remarks>
    /// Le nombre renvoyé est celui des messages réellement expédiés, pas celui des
    /// messages rencontrés. Un message qui échoue est repoussé à une échéance future ou
    /// abandonné : le compter comme traité ferait croire au relais qu'il reste du travail
    /// immédiat, et il enchaînerait les tours sans attendre.
    /// </remarks>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Nombre de messages expédiés.</returns>
    public async Task<int> DrainAsync(CancellationToken cancellationToken = default)
    {
        var nowUtc = _clock.GetUtcNow().UtcDateTime;

        // Le lot est chargé avec suivi : MarkSent et ScheduleRetry modifient des
        // propriétés, et une requête sans suivi ne verrait jamais la ligne être écrite.
        var due = await _unitOfWork.Repository<OutboxMessage>().TrackedQuery
            .Where(message => message.Status == OutboxMessageStatus.Pending
                && message.NextAttemptAtUtc <= nowUtc)
            .OrderBy(message => message.NextAttemptAtUtc)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        var expedes = 0;

        foreach (var message in due)
        {
            if (await TrySendAsync(message, cancellationToken))
            {
                expedes++;
            }
        }

        return expedes;
    }

    private async Task<bool> TrySendAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            await _sender.SendAsync(
                message.Recipient,
                new EmailMessage(message.Subject, message.HtmlBody, message.TextBody),
                cancellationToken);

            message.MarkSent(_clock.GetUtcNow().UtcDateTime);
        }
#pragma warning disable CA1031 // L'échec d'un message ne doit pas interrompre le lot.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            var willRetry = message.ScheduleRetry(
                ex.Message,
                _options.MaxAttempts,
                _clock.GetUtcNow().UtcDateTime);

            if (willRetry)
            {
                LogNouvelEssai(
                    _logger,
                    ex,
                    message.Id,
                    message.Recipient,
                    message.Kind,
                    message.Attempts);
            }
            else
            {
                LogAbandon(
                    _logger,
                    ex,
                    message.Id,
                    message.Recipient,
                    message.Kind,
                    message.Attempts);
            }

            // L'échec est écrit comme le succès : la date de reprise, ou l'abandon, doit
            // être persistée, sinon le message recommencerait à chaque tour.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return false;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    [LoggerMessage(
        EventId = 4110,
        Level = LogLevel.Warning,
        Message = "Échec de l'envoi du courriel {OutboxMessageId} à {Recipient} (modèle {Kind}), tentative {Attempts}.")]
    private static partial void LogNouvelEssai(
        ILogger logger,
        Exception exception,
        Guid outboxMessageId,
        string recipient,
        string kind,
        int attempts);

    [LoggerMessage(
        EventId = 4111,
        Level = LogLevel.Error,
        Message = "Le courriel {OutboxMessageId} à {Recipient} (modèle {Kind}) est abandonné après {Attempts} tentatives.")]
    private static partial void LogAbandon(
        ILogger logger,
        Exception exception,
        Guid outboxMessageId,
        string recipient,
        string kind,
        int attempts);
}
