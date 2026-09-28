using DotnetNiger.Domain.Common;

namespace DotnetNiger.Domain.Entities.Community;

/// <summary>
/// Message déposé par un visiteur via le formulaire de contact.
/// </summary>
/// <remarks>
/// Le message est conservé, et non seulement envoyé par courriel. Une adresse peut être
/// fausse, un filtre peut avaler le courriel : sans trace en base, un visiteur qui attend
/// une réponse n'aurait aucun moyen de le signaler, ni l'administration de le retrouver.
/// <para>
/// L'auteur n'est pas nécessairement un membre inscrit — c'est souvent tout l'intérêt du
/// formulaire — et son adresse n'est donc pas une clé étrangère vers un compte. Aucun
/// compte ne doit être créé pour prendre contact.
/// </para>
/// </remarks>
public sealed class ContactMessage : SoftDeletableEntity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private ContactMessage()
    {
    }

    private ContactMessage(string name, string email, string subject, string body, DateTime receivedAtUtc)
    {
        Name = DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(name, nameof(name)),
            FieldLimits.Name,
            nameof(name));

        Email = DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(email, nameof(email)),
            FieldLimits.EmailAddress,
            nameof(email));

        Subject = DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(subject, nameof(subject)),
            FieldLimits.Subject,
            nameof(subject));

        Body = DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(body, nameof(body)),
            FieldLimits.Content,
            nameof(body));

        ReceivedAtUtc = receivedAtUtc;
    }

    /// <summary>
    /// Dépose un message de contact.
    /// </summary>
    /// <param name="name">Nom de l'auteur du message.</param>
    /// <param name="email">Adresse à laquelle répondre.</param>
    /// <param name="subject">Objet du message.</param>
    /// <param name="body">Corps du message.</param>
    /// <param name="receivedAtUtc">Instant de réception (UTC).</param>
    /// <returns>Nouveau message, en attente de traitement.</returns>
    public static ContactMessage Create(
        string name,
        string email,
        string subject,
        string body,
        DateTime receivedAtUtc) =>
        new(name, email, subject, body, receivedAtUtc);

    /// <summary>
    /// Obtient le nom de l'auteur du message.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient l'adresse de courriel de l'auteur.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient l'objet du message.
    /// </summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient le corps du message.
    /// </summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient l'instant de réception (UTC).
    /// </summary>
    public DateTime ReceivedAtUtc { get; private set; }

    /// <summary>
    /// Obtient l'instant de traitement (UTC), ou <see langword="null"/> si le message est en attente.
    /// </summary>
    /// <remarks>
    /// Le traitement n'efface rien : un message reste consultable après y avoir répondu, ce
    /// qui laisse la trace de ce qui a été dit et de quand. Le retirer reste possible, et
    /// comme tout le reste du site cette retrait est logique, non destructive.
    /// </remarks>
    public DateTime? HandledAtUtc { get; private set; }

    /// <summary>
    /// Indique si le message a déjà reçu une réponse.
    /// </summary>
    public bool IsHandled => HandledAtUtc is not null;

    /// <summary>
    /// Marque le message comme traité.
    /// </summary>
    /// <param name="handledAtUtc">Instant du traitement (UTC).</param>
    public void MarkHandled(DateTime handledAtUtc) => HandledAtUtc = handledAtUtc;
}
