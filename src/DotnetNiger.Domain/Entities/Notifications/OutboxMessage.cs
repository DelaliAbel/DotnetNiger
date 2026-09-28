using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Entities.Notifications;

/// <summary>
/// Courriel retenu jusqu'à son expédition réelle.
/// </summary>
/// <remarks>
/// L'API n'appelle jamais le serveur SMTP depuis une requête HTTP : elle écrit cette ligne
/// dans la même transaction que le changement métier qui l'a motivée. Un message en base
/// signifie donc « cet événement a eu lieu » : si le serveur tombe ensuite, le relais de
/// fond repartira de ce qui reste, et l'utilisateur ne se voit jamais annoncer un envoi
/// qui n'a pas eu lieu.
/// <para>
/// Le corps est rendu au moment de l'écriture et non au moment de l'envoi : l'aperçu
/// affiché par l'administration correspond alors exactement à ce qui partira, et un
/// changement de modèle ultérieur ne réécrit pas rétroactivement des messages en attente.
/// </para>
/// </remarks>
public sealed class OutboxMessage : TimestampedEntity
{
    /// <summary>Intervalle avant la première nouvelle tentative.</summary>
    public static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);

    /// <summary>Plafond de l'attente entre deux tentatives.</summary>
    public static readonly TimeSpan MaxRetryDelay = TimeSpan.FromHours(1);

    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private OutboxMessage()
    {
    }

    /// <summary>
    /// Initialise un message prêt à partir dès la validation de la transaction.
    /// </summary>
    /// <param name="kind">Clé du modèle qui a produit ce message.</param>
    /// <param name="recipient">Adresse du destinataire.</param>
    /// <param name="subject">Objet du message.</param>
    /// <param name="htmlBody">Corps au format HTML.</param>
    /// <param name="textBody">Corps au format texte brut.</param>
    /// <param name="nowUtc">Instant de mise en file.</param>
    /// <param name="broadcastId">Diffusion à laquelle ce message se rattache, le cas échéant.</param>
    public OutboxMessage(
        string kind,
        string recipient,
        string subject,
        string htmlBody,
        string textBody,
        DateTime nowUtc,
        Guid? broadcastId = null)
    {
        Kind = Bounded(DomainGuard.AgainstNullOrWhiteSpace(kind, nameof(kind)), FieldLimits.Key, nameof(kind));
        Recipient = Bounded(DomainGuard.AgainstNullOrWhiteSpace(recipient, nameof(recipient)), FieldLimits.EmailAddress, nameof(recipient));
        Subject = Bounded(DomainGuard.AgainstNullOrWhiteSpace(subject, nameof(subject)), FieldLimits.Subject, nameof(subject));
        HtmlBody = DomainGuard.AgainstNullOrWhiteSpace(htmlBody, nameof(htmlBody));
        TextBody = DomainGuard.AgainstNullOrWhiteSpace(textBody, nameof(textBody));
        BroadcastId = broadcastId;
        Status = OutboxMessageStatus.Pending;
        NextAttemptAtUtc = nowUtc;
    }

    /// <summary>Clé du modèle à l'origine du message, pour le regrouper lors d'un diagnostic.</summary>
    public string Kind { get; private set; } = string.Empty;

    /// <summary>Adresse du destinataire.</summary>
    public string Recipient { get; private set; } = string.Empty;

    /// <summary>Objet du message.</summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>Corps au format HTML, rendu au moment de la mise en file.</summary>
    public string HtmlBody { get; private set; } = string.Empty;

    /// <summary>
    /// Corps au format texte brut, rendu au moment de la mise en file.
    /// </summary>
    /// <remarks>
    /// Il est stocké avec le corps HTML, et non reconstruit à l'envoi : la version texte
    /// d'un message doit être exactement celle qui a été prévisualisée, comme le HTML.
    /// </remarks>
    public string TextBody { get; private set; } = string.Empty;

    /// <summary>État courant du message.</summary>
    public OutboxMessageStatus Status { get; private set; }

    /// <summary>Nombre de tentatives d'expédition déjà consommées.</summary>
    public int Attempts { get; private set; }

    /// <summary>Instant de la prochaine tentative, ou <see langword="null"/> si le message est clos.</summary>
    public DateTime? NextAttemptAtUtc { get; private set; }

    /// <summary>Instant d'expédition réussie.</summary>
    public DateTime? SentAtUtc { get; private set; }

    /// <summary>Motif du dernier échec, conservé pour le diagnostic.</summary>
    public string? LastError { get; private set; }

    /// <summary>Diffusion à laquelle se rattache ce message, s'il en fait partie.</summary>
    public Guid? BroadcastId { get; private set; }

    /// <summary>
    /// Indique si le message est prêt à être expédié à l'instant donné.
    /// </summary>
    /// <param name="nowUtc">Instant courant.</param>
    /// <returns><see langword="true"/> si une tentative est due.</returns>
    public bool IsDue(DateTime nowUtc) =>
        Status == OutboxMessageStatus.Pending
        && NextAttemptAtUtc <= nowUtc;

    /// <summary>
    /// Marque le message comme expédié.
    /// </summary>
    /// <param name="nowUtc">Instant d'expédition.</param>
    public void MarkSent(DateTime nowUtc)
    {
        Status = OutboxMessageStatus.Sent;
        SentAtUtc = nowUtc;
        NextAttemptAtUtc = null;
        LastError = null;
    }

    /// <summary>
    /// Consigne un échec et reprogramme une nouvelle tentative, ou abandonne le message
    /// si les tentatives sont épuisées.
    /// </summary>
    /// <param name="error">Motif de l'échec.</param>
    /// <param name="maxAttempts">Nombre de tentatives avant abandon.</param>
    /// <param name="nowUtc">Instant de l'échec.</param>
    /// <returns><see langword="true"/> si une nouvelle tentative est programmée.</returns>
    public bool ScheduleRetry(string error, int maxAttempts, DateTime nowUtc)
    {
        LastError = Bounded(DomainGuard.AgainstNullOrWhiteSpace(error, nameof(error)), FieldLimits.Body, nameof(error));
        Attempts++;

        if (Attempts >= maxAttempts)
        {
            Status = OutboxMessageStatus.Failed;
            NextAttemptAtUtc = null;
            return false;
        }

        // Attente exponentielle plafonnée : une panne passagère du serveur de courriel se
        // résorbe d'elle-même, sans marteler un service qui ne répond pas.
        var delay = TimeSpan.FromTicks(RetryDelay.Ticks * (1L << Math.Min(Attempts - 1, 16)));

        if (delay > MaxRetryDelay)
        {
            delay = MaxRetryDelay;
        }

        NextAttemptAtUtc = nowUtc.Add(delay);
        return true;
    }

    private static string Bounded(string value, int maxLength, string field) =>
        DomainGuard.AgainstTooLong(value, maxLength, field);
}
