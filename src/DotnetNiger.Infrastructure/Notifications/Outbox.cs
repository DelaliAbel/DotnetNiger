using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Notifications;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Infrastructure.Notifications;

/// <summary>
/// Écriture des courriels en attente dans la file d'attente.
/// </summary>
/// <remarks>
/// La validation est laissée à l'appelant : c'est ce qui garantit que le message et le
/// changement métier partagent la même transaction.
/// </remarks>
/// <param name="unitOfWork">Unité de travail de l'appelant.</param>
/// <param name="clock">Source de temps, afin que les horodatages soient testables.</param>
internal sealed class Outbox(IUnitOfWork unitOfWork, TimeProvider clock) : IOutbox
{
    private readonly IRepository<OutboxMessage> _messages = unitOfWork.Repository<OutboxMessage>();

    /// <inheritdoc />
    public Task EnqueueAsync(string kind, QueuedEmail email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        cancellationToken.ThrowIfCancellationRequested();

        _messages.Add(new OutboxMessage(
            kind,
            email.Recipient,
            email.Message.Subject,
            email.Message.HtmlBody,
            email.Message.TextBody,
            clock.GetUtcNow().UtcDateTime));

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> EnqueueManyAsync(
        string kind,
        IReadOnlyList<QueuedEmail> emails,
        Guid? broadcastId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(emails);

        if (emails.Count == 0)
        {
            return Task.FromResult(0);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var nowUtc = clock.GetUtcNow().UtcDateTime;

        foreach (var email in emails)
        {
            ArgumentNullException.ThrowIfNull(email);

            _messages.Add(new OutboxMessage(
                kind,
                email.Recipient,
                email.Message.Subject,
                email.Message.HtmlBody,
            email.Message.TextBody,
                nowUtc,
                broadcastId));
        }

        return Task.FromResult(emails.Count);
    }
}
